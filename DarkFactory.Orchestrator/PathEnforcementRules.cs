namespace DarkFactory.Orchestrator;

/// <summary>
/// Pure path-enforcement rules for each agent role.
/// No SDK dependency — fully unit-testable.
/// </summary>
public static class PathEnforcementRules
{
    private static readonly HashSet<string> WritingToolNames = new(StringComparer.OrdinalIgnoreCase)
    {
        // Copilot CLI write tools (snake_case canonical names + legacy PascalCase variants)
        "write_file", "edit_file", "multiedit_file",
        "Write", "Edit", "MultiEdit",
    };

    /// <summary>Evaluate whether <paramref name="role"/> may write to <paramref name="filePath"/>.</summary>
    /// <param name="role">The agent role requesting the write.</param>
    /// <param name="toolName">The name of the tool being invoked.</param>
    /// <param name="filePath">Relative file path (forward slashes, no leading slash).</param>
    /// <returns>An <see cref="EnforcementResult"/> with the allow/deny decision and reason.</returns>
    public static EnforcementResult Evaluate(AgentRole role, string toolName, string? filePath)
    {
        if (!WritingToolNames.Contains(toolName))
            return EnforcementResult.Allowed;

        if (filePath is null)
            return EnforcementResult.Allowed;

        var normalized = Normalize(filePath);

        return role switch
        {
            AgentRole.BackendDeveloper  => EvaluateBackendDeveloper(normalized),
            AgentRole.FrontendDeveloper => EvaluateFrontendDeveloper(normalized),
            AgentRole.BackendTesting    => EvaluateBackendTesting(normalized),
            AgentRole.FrontendTesting   => EvaluateFrontendTesting(normalized),
            AgentRole.Reviewer          => EnforcementResult.Deny($"Reviewer may not write files: {normalized}"),
            AgentRole.PoVerifier        => EnforcementResult.Deny($"PoVerifier may not write files: {normalized}"),
            _ => EnforcementResult.Allowed,
        };
    }

    private static EnforcementResult EvaluateBackendDeveloper(string path)
    {
        if (path.StartsWith("DarkFactory.Weather.Tests/", StringComparison.OrdinalIgnoreCase))
            return EnforcementResult.Deny(
                $"developer-backend-agent must not modify test files. Path: {path}");

        if (!path.StartsWith("DarkFactory.Weather/", StringComparison.OrdinalIgnoreCase))
            return EnforcementResult.Deny(
                $"developer-backend-agent may only write to DarkFactory.Weather/. Path: {path}");

        return EnforcementResult.Allowed;
    }

    private static EnforcementResult EvaluateFrontendDeveloper(string path)
    {
        if (!path.StartsWith("dark-factory-ui/", StringComparison.OrdinalIgnoreCase))
            return EnforcementResult.Deny(
                $"developer-frontend-agent may only write to dark-factory-ui/. Path: {path}");

        if (path.StartsWith("dark-factory-ui/node_modules/", StringComparison.OrdinalIgnoreCase))
            return EnforcementResult.Deny(
                $"developer-frontend-agent must not write to node_modules/. Path: {path}");

        if (IsTestFile(Path.GetFileName(path)))
            return EnforcementResult.Deny(
                $"developer-frontend-agent must not modify test files. Path: {path}");

        return EnforcementResult.Allowed;
    }

    private static EnforcementResult EvaluateBackendTesting(string path)
    {
        if (!path.StartsWith("DarkFactory.Weather.Tests/", StringComparison.OrdinalIgnoreCase))
            return EnforcementResult.Deny(
                $"testing-backend-agent may only write to DarkFactory.Weather.Tests/. Path: {path}");

        return EnforcementResult.Allowed;
    }

    private static EnforcementResult EvaluateFrontendTesting(string path)
    {
        if (!path.StartsWith("dark-factory-ui/", StringComparison.OrdinalIgnoreCase))
            return EnforcementResult.Deny(
                $"testing-frontend-agent may only write to dark-factory-ui/. Path: {path}");

        if (path.StartsWith("dark-factory-ui/node_modules/", StringComparison.OrdinalIgnoreCase))
            return EnforcementResult.Deny(
                $"testing-frontend-agent must not write to node_modules/. Path: {path}");

        var basename = Path.GetFileName(path);
        if (IsTestFile(basename) || IsVitestConfig(basename) || basename == "package.json")
            return EnforcementResult.Allowed;

        return EnforcementResult.Deny(
            $"testing-frontend-agent may only write test files, vitest config, or package.json. Path: {path}");
    }

    private static bool IsTestFile(string basename) =>
        basename.EndsWith(".test.ts", StringComparison.OrdinalIgnoreCase) ||
        basename.EndsWith(".test.tsx", StringComparison.OrdinalIgnoreCase) ||
        basename.EndsWith(".spec.ts", StringComparison.OrdinalIgnoreCase) ||
        basename.EndsWith(".spec.tsx", StringComparison.OrdinalIgnoreCase);

    private static bool IsVitestConfig(string basename) =>
        basename is "vitest.config.ts" or "vitest.config.js"
            or "test-setup.ts" or "test-setup.js"
            or "setup.ts" or "setup.js";

    /// <summary>Normalise to forward-slash, relative (strip leading slash).</summary>
    internal static string Normalize(string path)
    {
        var normalized = path.Replace('\\', '/');
        return normalized.TrimStart('/');
    }
}

/// <summary>Result of a path-enforcement evaluation.</summary>
/// <param name="IsAllowed">Whether the write is permitted.</param>
/// <param name="DenyReason">Human-readable reason if denied; null when allowed.</param>
public record EnforcementResult(bool IsAllowed, string? DenyReason)
{
    /// <summary>The write is permitted.</summary>
    public static readonly EnforcementResult Allowed = new(true, null);

    /// <summary>Create a denied result with the supplied reason.</summary>
    public static EnforcementResult Deny(string reason) => new(false, reason);
}
