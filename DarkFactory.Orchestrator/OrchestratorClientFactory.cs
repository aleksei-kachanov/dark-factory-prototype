using GitHub.Copilot.SDK;

namespace DarkFactory.Orchestrator;

/// <summary>Creates <see cref="CopilotClient"/> instances with role-appropriate telemetry config.</summary>
public static class OrchestratorClientFactory
{
    /// <summary>
    /// Build telemetry config that writes JSONL traces to
    /// <c>docs/pipeline/metrics/traces/{issueNumber}.jsonl</c>.
    /// </summary>
    /// <param name="issueNumber">The GitHub issue number used as the trace file name.</param>
    /// <param name="repoRoot">Absolute or relative path to the repository root. Defaults to <c>"."</c>.</param>
    public static TelemetryConfig BuildTelemetryConfig(int issueNumber, string repoRoot = ".")
    {
        var path = Path.GetFullPath(
            Path.Combine(repoRoot, "docs", "pipeline", "metrics", "traces", $"{issueNumber}.jsonl"));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        return new TelemetryConfig
        {
            FilePath = path,
            ExporterType = "file",
            CaptureContent = false,
        };
    }

    /// <summary>Create a <see cref="CopilotClient"/> with file-based telemetry for the given issue.</summary>
    /// <param name="issueNumber">The GitHub issue number used as the trace file name.</param>
    /// <param name="repoRoot">Absolute or relative path to the repository root. Defaults to <c>"."</c>.</param>
    public static CopilotClient CreateWithTelemetry(int issueNumber, string repoRoot = ".") =>
        new(new CopilotClientOptions
        {
            Telemetry = BuildTelemetryConfig(issueNumber, repoRoot),
        });
}
