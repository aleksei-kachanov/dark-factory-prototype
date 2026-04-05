using GitHub.Copilot.SDK;

namespace DarkFactory.Orchestrator;

/// <summary>
/// Creates <see cref="SessionConfig"/> instances with role-appropriate SDK configuration.
/// Phase 1 wires path-enforcement hooks only; Phase 2 branches extend this with
/// permission handlers, custom tools, system-message injection, and infinite-session config.
/// </summary>
public static class SessionFactory
{
    /// <summary>
    /// Create a <see cref="SessionConfig"/> pre-configured for <paramref name="role"/>.
    /// Callers must provide an <paramref name="onPermissionRequest"/> handler as required by the SDK.
    /// </summary>
    /// <param name="role">The agent role that will run in this session.</param>
    /// <param name="onPermissionRequest">
    /// Permission handler required by the SDK. Use <see cref="PermissionHandler.ApproveAll"/> for
    /// non-sensitive sessions; supply a role-specific handler (Phase 2) for fine-grained control.
    /// </param>
    public static SessionConfig ForRole(AgentRole role,
        PermissionRequestHandler? onPermissionRequest = null) => new()
    {
        Hooks = PathEnforcementHooks.ForRole(role),
        OnPermissionRequest = onPermissionRequest ?? PermissionHandler.ApproveAll,
        InfiniteSessions = role is AgentRole.BackendDeveloper or AgentRole.FrontendDeveloper
            ? DeveloperInfiniteSessions.Create()
            : null,
    };
}
