---
description: "Use when reviewing pull requests, performing code review, or evaluating changes in the DarkFactory.Weather project. Covers PR review checklist, C# standards, API design, test requirements, and security."
---

# DarkFactory Pull Request Review Guidelines

## Architecture & Design

- Follow the Controller → Service → Model layering; do not put business logic in controllers.
- All dependencies must be injected via constructor injection — no service locator, no `static` state in production code.
- New services must be registered in `Program.cs` with the appropriate lifetime:
  - `AddScoped` for per-request state (default for services).
  - `AddSingleton` for stateless, thread-safe services.
  - `AddTransient` for lightweight, stateless utilities.
- DTOs (`*Dto`) must be used in controller responses; domain models (`Models/`) must not leak into the API layer.
- New domain models should use C# `record` with positional constructors (see `WeatherForecast.cs`).

## API Design

- All endpoints must follow the versioned route pattern: `api/v{version:apiVersion}/{resource}`.
- New endpoints must declare `[ProducesResponseType]` for every possible HTTP status code.
- Return `400 BadRequest` for invalid/missing input; validate with `string.IsNullOrWhiteSpace` or FluentValidation.
- Input strings coming from the route or query must be validated before being passed to services.
- Do not expose internal exception details to the caller; return problem details or a safe error message.

## C# Code Standards

- Use `var` only when the type is obvious from the right-hand side.
- Prefer expression-bodied members for single-line properties/methods.
- Use collection expressions (`[...]`) instead of `new T[] { ... }` for array literals (C# 12+).
- Use `StringComparer.OrdinalIgnoreCase` for case-insensitive dictionary/string comparisons.
- Avoid `Random.Shared` in new production code if determinism or testability is required; prefer injected randomness.
- No `#region` blocks; keep files short and focused.
- Prefer `Math.Clamp` over manual min/max guards.

## Testing

- Every new public method on a service or controller must have at least one unit test.
- Tests must follow the Arrange / Act / Assert structure; no mixed logic.
- Use `Moq` for all dependency mocking; never instantiate real services in controller tests.
- Cover both the happy path and edge cases (empty input, boundary values, unknown region, etc.).
- Test method names must follow the pattern: `MethodName_Condition_ExpectedBehavior`.
- Tests must not rely on real time (`DateTime.Now`); use `DateTime.UtcNow` and consider making it injectable for determinism.
- Do not use `Thread.Sleep` or `Task.Delay` in tests.

## Security (OWASP Top 10)

- **Injection**: Never concatenate user input into file paths, log statements, or external calls without sanitisation.
- **Input Validation**: All route parameters and query strings must be validated for length and allowed character set before use.
- **Sensitive data**: Do not log request payload contents; log only structured, non-sensitive metadata.
- **Dependency versions**: Flag any new NuGet packages with known CVEs or that were added without justification.
- **Error handling**: Exceptions must be caught at the middleware level; service methods should not swallow exceptions silently.

## Documentation

- Every new public controller action must have an XML `<summary>` and `<param>` comment (used by Swagger).
- New service interface members must have XML doc comments describing their contract and any thrown exceptions.
- Update `appsettings.json` schema comments if new configuration keys are added.

## Pull Request Checklist

Before approving, confirm:

- [ ] All new code has corresponding unit tests and they pass (`dotnet test DarkFactory.slnx`).
- [ ] No business logic lives in controllers.
- [ ] Input validation is present for all new endpoints.
- [ ] No secrets, connection strings, or credentials are hard-coded.
- [ ] No unnecessary `using` directives or dead code.
- [ ] Swagger annotations are up to date.
- [ ] New NuGet packages are justified in the PR description.
