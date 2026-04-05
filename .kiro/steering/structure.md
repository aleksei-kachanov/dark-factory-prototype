# Project Structure

```
DarkFactory.slnx                  # .NET solution file
DarkFactory.Weather/              # ASP.NET Core Web API
  Controllers/                    # API controllers (one per resource)
  Services/                       # Business logic — interface + implementation pairs
  Models/                         # Internal domain records (not exposed directly)
  Dtos/                           # Public-facing response shapes
  Program.cs                      # App bootstrap / DI registration
  ConfigureSwaggerOptions.cs      # Per-version Swagger doc configuration
DarkFactory.Weather.Tests/        # xUnit test project
  WeatherControllerTests.cs       # Controller-level tests (uses Mvc.Testing)
  WeatherServiceTests.cs          # Unit tests for service logic
dark-factory-ui/                  # React + TypeScript SPA
  src/
    components/                   # UI components (each has a paired .css file)
    types/                        # Shared TypeScript interfaces
    App.tsx                       # Root component
    main.tsx                      # Entry point
docs/                             # Project documentation
.github/
  workflows/                      # CI/CD pipelines
  agents/                         # AI agent instruction files
  instructions/                   # Additional AI coding instructions
.kiro/steering/                   # Kiro steering files (this directory)
```

## Conventions

- Services follow the interface/implementation pattern: `IXxxService` + `XxxService`, registered as `Scoped`.
- Domain models live in `Models/` as C# `record` types; they are never returned directly from controllers.
- DTOs in `Dtos/` are also `record` types and are the only types serialised to JSON responses.
- Controllers use primary-constructor injection and route via `api/v{version:apiVersion}/resource`.
- All new API endpoints must declare `[ProducesResponseType]` attributes for documented status codes.
- Frontend components each own their styles in a co-located `.css` file.
- TypeScript strict mode is on; avoid `any`.
