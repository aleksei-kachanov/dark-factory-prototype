using DarkFactory.Orchestrator;

namespace DarkFactory.Orchestrator.Tests;

public class InfiniteSessionsConfigTests
{
    // ─── DeveloperInfiniteSessions ───────────────────────────────────────────

    [Fact]
    public void Create_HasCorrectThresholds()
    {
        var config = DeveloperInfiniteSessions.Create();

        Assert.NotNull(config.BackgroundCompactionThreshold);
        Assert.NotNull(config.BufferExhaustionThreshold);
        Assert.True(config.Enabled);
        Assert.Equal(0.80d, config.BackgroundCompactionThreshold!.Value, precision: 2);
        Assert.Equal(0.95d, config.BufferExhaustionThreshold!.Value, precision: 2);
    }

    [Fact]
    public void Create_ReturnsFreshInstanceEachCall()
    {
        var a = DeveloperInfiniteSessions.Create();
        var b = DeveloperInfiniteSessions.Create();

        Assert.NotSame(a, b);
    }

    // ─── OrchestratorClientFactory ───────────────────────────────────────────

    [Fact]
    public void BuildTelemetryConfig_ReturnsCorrectFilePath()
    {
        var testRoot = Path.Combine(Directory.GetCurrentDirectory(), $"test-traces-{Guid.NewGuid():N}");
        try
        {
            var config = OrchestratorClientFactory.BuildTelemetryConfig(42, testRoot);

            Assert.Contains("traces", config.FilePath);
            Assert.Contains("42.jsonl", config.FilePath);
            Assert.Equal("file", config.ExporterType);
            Assert.False(config.CaptureContent);
        }
        finally
        {
            if (Directory.Exists(testRoot)) Directory.Delete(testRoot, recursive: true);
        }
    }

    [Fact]
    public void BuildTelemetryConfig_CreatesTracesDirectory()
    {
        var testRoot = Path.Combine(Directory.GetCurrentDirectory(), $"test-traces-{Guid.NewGuid():N}");
        try
        {
            var config = OrchestratorClientFactory.BuildTelemetryConfig(99, testRoot);

            Assert.True(Directory.Exists(Path.GetDirectoryName(config.FilePath)));
        }
        finally
        {
            if (Directory.Exists(testRoot)) Directory.Delete(testRoot, recursive: true);
        }
    }

    [Fact]
    public void BuildTelemetryConfig_ReturnsAbsoluteFilePath()
    {
        var testRoot = Path.Combine(Directory.GetCurrentDirectory(), $"test-traces-{Guid.NewGuid():N}");
        try
        {
            var config = OrchestratorClientFactory.BuildTelemetryConfig(1, testRoot);

            Assert.True(Path.IsPathRooted(config.FilePath));
        }
        finally
        {
            if (Directory.Exists(testRoot)) Directory.Delete(testRoot, recursive: true);
        }
    }

    // ─── SessionFactory role branching (Theory) ──────────────────────────────

    [Theory]
    [InlineData(AgentRole.BackendDeveloper)]
    [InlineData(AgentRole.FrontendDeveloper)]
    public void ForRole_DeveloperRoles_HaveInfiniteSessionsEnabled(AgentRole role)
    {
        var config = SessionFactory.ForRole(role);

        Assert.NotNull(config.InfiniteSessions);
        Assert.True(config.InfiniteSessions.Enabled);
    }

    [Theory]
    [InlineData(AgentRole.BackendTesting)]
    [InlineData(AgentRole.FrontendTesting)]
    [InlineData(AgentRole.Reviewer)]
    [InlineData(AgentRole.PoVerifier)]
    [InlineData(AgentRole.Devops)]
    [InlineData(AgentRole.Telemetry)]
    [InlineData(AgentRole.PipelineAnalyst)]
    public void ForRole_NonDeveloperRoles_HaveNoInfiniteSessions(AgentRole role)
    {
        var config = SessionFactory.ForRole(role);

        Assert.Null(config.InfiniteSessions);
    }

    // ─── SessionFactory role branching (named Facts) ─────────────────────────

    [Fact]
    public void SessionFactory_BackendDeveloper_HasInfiniteSessions()
    {
        var config = SessionFactory.ForRole(AgentRole.BackendDeveloper);

        Assert.NotNull(config.InfiniteSessions);
        Assert.True(config.InfiniteSessions.Enabled);
    }

    [Fact]
    public void SessionFactory_FrontendDeveloper_HasInfiniteSessions()
    {
        var config = SessionFactory.ForRole(AgentRole.FrontendDeveloper);

        Assert.NotNull(config.InfiniteSessions);
        Assert.True(config.InfiniteSessions.Enabled);
    }

    [Fact]
    public void SessionFactory_Reviewer_NoInfiniteSessions()
    {
        var config = SessionFactory.ForRole(AgentRole.Reviewer);

        Assert.Null(config.InfiniteSessions);
    }
}
