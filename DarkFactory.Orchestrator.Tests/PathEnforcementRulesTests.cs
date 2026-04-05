using DarkFactory.Orchestrator;

namespace DarkFactory.Orchestrator.Tests;

public class PathEnforcementRulesTests
{
    // ─── BackendDeveloper ────────────────────────────────────────────────────

    [Theory]
    [InlineData("write_file", "DarkFactory.Weather/Controllers/WeatherController.cs")]
    [InlineData("edit_file",  "DarkFactory.Weather/Services/WeatherService.cs")]
    [InlineData("Write",      "DarkFactory.Weather/Program.cs")]
    public void Evaluate_BackendDeveloper_AllowedPath_ReturnsAllowed(string tool, string path)
    {
        var result = PathEnforcementRules.Evaluate(AgentRole.BackendDeveloper, tool, path);

        Assert.True(result.IsAllowed);
        Assert.Null(result.DenyReason);
    }

    [Theory]
    [InlineData("write_file", "DarkFactory.Weather.Tests/WeatherServiceTests.cs")]
    [InlineData("edit_file",  "DarkFactory.Weather.Tests/WeatherControllerTests.cs")]
    public void Evaluate_BackendDeveloper_TestPath_ReturnsDenied(string tool, string path)
    {
        var result = PathEnforcementRules.Evaluate(AgentRole.BackendDeveloper, tool, path);

        Assert.False(result.IsAllowed);
        Assert.Contains("must not modify test files", result.DenyReason);
    }

    [Theory]
    [InlineData("write_file", "dark-factory-ui/src/App.tsx")]
    [InlineData("edit_file",  "docs/pipeline/shared-gates.md")]
    [InlineData("write_file", ".github/agents/developer-backend-darkfactory.agent.md")]
    public void Evaluate_BackendDeveloper_OutOfScopePath_ReturnsDenied(string tool, string path)
    {
        var result = PathEnforcementRules.Evaluate(AgentRole.BackendDeveloper, tool, path);

        Assert.False(result.IsAllowed);
        Assert.Contains("may only write to DarkFactory.Weather/", result.DenyReason);
    }

    [Fact]
    public void Evaluate_BackendDeveloper_ReadTool_ReturnsAllowed()
    {
        // Non-write tools must always pass through regardless of path
        var result = PathEnforcementRules.Evaluate(
            AgentRole.BackendDeveloper, "read_file", "dark-factory-ui/src/App.tsx");

        Assert.True(result.IsAllowed);
    }

    // ─── FrontendDeveloper ───────────────────────────────────────────────────

    [Theory]
    [InlineData("write_file", "dark-factory-ui/src/App.tsx")]
    [InlineData("edit_file",  "dark-factory-ui/src/components/RegionSelector.tsx")]
    [InlineData("write_file", "dark-factory-ui/package.json")]
    public void Evaluate_FrontendDeveloper_AllowedPath_ReturnsAllowed(string tool, string path)
    {
        var result = PathEnforcementRules.Evaluate(AgentRole.FrontendDeveloper, tool, path);

        Assert.True(result.IsAllowed);
    }

    [Fact]
    public void Evaluate_FrontendDeveloper_NodeModules_ReturnsDenied()
    {
        var result = PathEnforcementRules.Evaluate(
            AgentRole.FrontendDeveloper, "write_file",
            "dark-factory-ui/node_modules/react/index.js");

        Assert.False(result.IsAllowed);
        Assert.Contains("node_modules", result.DenyReason);
    }

    [Theory]
    [InlineData("dark-factory-ui/src/App.test.tsx")]
    [InlineData("dark-factory-ui/src/components/ForecastTable.spec.ts")]
    public void Evaluate_FrontendDeveloper_TestFile_ReturnsDenied(string path)
    {
        var result = PathEnforcementRules.Evaluate(AgentRole.FrontendDeveloper, "write_file", path);

        Assert.False(result.IsAllowed);
        Assert.Contains("must not modify test files", result.DenyReason);
    }

    [Fact]
    public void Evaluate_FrontendDeveloper_BackendPath_ReturnsDenied()
    {
        var result = PathEnforcementRules.Evaluate(
            AgentRole.FrontendDeveloper, "write_file",
            "DarkFactory.Weather/Controllers/WeatherController.cs");

        Assert.False(result.IsAllowed);
        Assert.Contains("may only write to dark-factory-ui/", result.DenyReason);
    }

    // ─── BackendTesting ──────────────────────────────────────────────────────

    [Theory]
    [InlineData("write_file", "DarkFactory.Weather.Tests/WeatherServiceTests.cs")]
    [InlineData("edit_file",  "DarkFactory.Weather.Tests/WeatherControllerTests.cs")]
    public void Evaluate_BackendTesting_AllowedPath_ReturnsAllowed(string tool, string path)
    {
        var result = PathEnforcementRules.Evaluate(AgentRole.BackendTesting, tool, path);

        Assert.True(result.IsAllowed);
    }

    [Fact]
    public void Evaluate_BackendTesting_ProductionPath_ReturnsDenied()
    {
        var result = PathEnforcementRules.Evaluate(
            AgentRole.BackendTesting, "write_file",
            "DarkFactory.Weather/Services/WeatherService.cs");

        Assert.False(result.IsAllowed);
        Assert.Contains("may only write to DarkFactory.Weather.Tests/", result.DenyReason);
    }

    // ─── FrontendTesting ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("dark-factory-ui/src/App.test.tsx")]
    [InlineData("dark-factory-ui/src/components/ForecastTable.spec.ts")]
    [InlineData("dark-factory-ui/vitest.config.ts")]
    [InlineData("dark-factory-ui/test-setup.ts")]
    [InlineData("dark-factory-ui/package.json")]
    public void Evaluate_FrontendTesting_AllowedPath_ReturnsAllowed(string path)
    {
        var result = PathEnforcementRules.Evaluate(AgentRole.FrontendTesting, "write_file", path);

        Assert.True(result.IsAllowed);
    }

    [Fact]
    public void Evaluate_FrontendTesting_ProductionComponent_ReturnsDenied()
    {
        var result = PathEnforcementRules.Evaluate(
            AgentRole.FrontendTesting, "write_file",
            "dark-factory-ui/src/components/RegionSelector.tsx");

        Assert.False(result.IsAllowed);
        Assert.Contains("may only write test files", result.DenyReason);
    }

    // ─── Reviewer / PoVerifier ───────────────────────────────────────────────

    [Theory]
    [InlineData(AgentRole.Reviewer,   "write_file", "DarkFactory.Weather/Services/WeatherService.cs")]
    [InlineData(AgentRole.Reviewer,   "edit_file",  "dark-factory-ui/src/App.tsx")]
    [InlineData(AgentRole.PoVerifier, "write_file", "DarkFactory.Weather/Models/WeatherForecast.cs")]
    [InlineData(AgentRole.PoVerifier, "MultiEdit",  "README.md")]
    public void Evaluate_ReviewerOrPoVerifier_AnyWrite_ReturnsDenied(
        AgentRole role, string tool, string path)
    {
        var result = PathEnforcementRules.Evaluate(role, tool, path);

        Assert.False(result.IsAllowed);
        Assert.NotNull(result.DenyReason);
    }

    [Theory]
    [InlineData(AgentRole.Reviewer)]
    [InlineData(AgentRole.PoVerifier)]
    public void Evaluate_ReviewerOrPoVerifier_ReadTool_ReturnsAllowed(AgentRole role)
    {
        var result = PathEnforcementRules.Evaluate(
            role, "read_file", "DarkFactory.Weather/Services/WeatherService.cs");

        Assert.True(result.IsAllowed);
    }

    // ─── Non-write tools ─────────────────────────────────────────────────────

    [Theory]
    [InlineData(AgentRole.Reviewer)]
    [InlineData(AgentRole.PoVerifier)]
    [InlineData(AgentRole.BackendDeveloper)]
    public void Evaluate_AnyRole_NonWriteTool_ReturnsAllowed(AgentRole role)
    {
        var result = PathEnforcementRules.Evaluate(role, "run_shell", null);

        Assert.True(result.IsAllowed);
    }

    // ─── Path normalisation ──────────────────────────────────────────────────

    [Fact]
    public void Evaluate_BackendDeveloper_AbsolutePath_NormalisedAndAllowed()
    {
        // Absolute paths should be normalised to relative (leading slash stripped)
        var result = PathEnforcementRules.Evaluate(
            AgentRole.BackendDeveloper, "write_file",
            "/DarkFactory.Weather/Controllers/WeatherController.cs");

        Assert.True(result.IsAllowed);
    }

    [Fact]
    public void Evaluate_BackendDeveloper_WindowsPath_NormalisedAndAllowed()
    {
        var result = PathEnforcementRules.Evaluate(
            AgentRole.BackendDeveloper, "write_file",
            "DarkFactory.Weather\\Controllers\\WeatherController.cs");

        Assert.True(result.IsAllowed);
    }

    // ─── Devops / other roles default to allow ───────────────────────────────

    [Fact]
    public void Evaluate_Devops_AnyPath_ReturnsAllowed()
    {
        var result = PathEnforcementRules.Evaluate(
            AgentRole.Devops, "write_file", "docs/pipeline/sdlc-pipeline.md");

        Assert.True(result.IsAllowed);
    }
}
