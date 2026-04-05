using GitHub.Copilot.SDK;

namespace DarkFactory.Orchestrator;

/// <summary>
/// Pre-built InfiniteSessionConfig and compaction event logging for developer sessions.
/// </summary>
public static class DeveloperInfiniteSessions
{
    /// <summary>Create a fresh infinite session config for developer agents (80% / 95% thresholds).</summary>
    public static InfiniteSessionConfig Create() => new()
    {
        Enabled = true,
        BackgroundCompactionThreshold = 0.80,
        BufferExhaustionThreshold = 0.95,
    };

    /// <summary>
    /// Subscribe to compaction lifecycle events on <paramref name="session"/> and log them.
    /// </summary>
    /// <param name="session">The active <see cref="CopilotSession"/> to observe.</param>
    /// <param name="log">Callback invoked with a structured log line for each compaction event.</param>
    /// <returns>An <see cref="IDisposable"/> that unsubscribes when disposed.</returns>
    public static IDisposable SubscribeCompactionEvents(
        CopilotSession session, Action<string> log) =>
        session.On(evt =>
        {
            switch (evt)
            {
                case SessionCompactionStartEvent start:
                    log($"[compaction-start] system={start.Data.SystemTokens} conversation={start.Data.ConversationTokens}");
                    break;
                case SessionCompactionCompleteEvent complete:
                    log($"[compaction-complete] success={complete.Data.Success} tokensRemoved={complete.Data.TokensRemoved} checkpoint={complete.Data.CheckpointNumber}");
                    break;
            }
        });
}
