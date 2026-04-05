using System.Text.Json;
using GitHub.Copilot.SDK;

namespace DarkFactory.Orchestrator;

/// <summary>
/// Thin SDK adapter that wires <see cref="PathEnforcementRules"/> into
/// <see cref="SessionHooks.OnPreToolUse"/> for a given <see cref="AgentRole"/>.
/// </summary>
public static class PathEnforcementHooks
{
    /// <summary>Build <see cref="SessionHooks"/> with path enforcement for <paramref name="role"/>.</summary>
    public static SessionHooks ForRole(AgentRole role) => new()
    {
        OnPreToolUse = (input, _) =>
        {
            var filePath = ExtractPath(input.ToolArgs);
            var result = PathEnforcementRules.Evaluate(role, input.ToolName, filePath);

            return Task.FromResult<PreToolUseHookOutput?>(new PreToolUseHookOutput
            {
                PermissionDecision = result.IsAllowed ? "allow" : "deny",
                PermissionDecisionReason = result.DenyReason,
            });
        },
    };

    /// <summary>
    /// Extract a file path from SDK tool arguments.
    /// At runtime <paramref name="toolArgs"/> is a <see cref="JsonElement"/> produced by the
    /// SDK's JSON deserialisation. Checks common parameter names used by Copilot CLI write tools.
    /// </summary>
    internal static string? ExtractPath(object? toolArgs)
    {
        if (toolArgs is not JsonElement element || element.ValueKind != JsonValueKind.Object)
            return null;

        foreach (var key in new[] { "path", "file_path", "filePath", "new_path" })
        {
            if (element.TryGetProperty(key, out var value) &&
                value.ValueKind == JsonValueKind.String)
            {
                return PathEnforcementRules.Normalize(value.GetString()!);
            }
        }

        return null;
    }
}
