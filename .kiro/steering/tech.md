# Tech Stack

## Backend
- Runtime: .NET 10 (C#)
- Framework: ASP.NET Core Web API
- API versioning: `Asp.Versioning.Mvc` — URL segment style (`/api/v{version}/...`)
- API docs: Swashbuckle (Swagger UI) — available at `/swagger` in Development
- Nullable reference types and implicit usings are enabled
- XML doc comments are generated and wired into Swagger

## Frontend
- Runtime: Node / browser
- Framework: React 19 with TypeScript
- Build tool: Vite 8
- Linting: ESLint 9 with `typescript-eslint` and `react-hooks` plugins

## Testing (backend)
- Framework: xUnit 2
- Mocking: Moq
- Integration testing: `Microsoft.AspNetCore.Mvc.Testing`
- Coverage: coverlet

## Solution
- Solution file: `DarkFactory.slnx` (new `.slnx` format)

---

## Common Commands

### Backend
```bash
# Build
dotnet build

# Run API (from repo root)
dotnet run --project DarkFactory.Weather

# Run tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend
```bash
cd dark-factory-ui

# Install dependencies
npm install

# Dev server (proxies API from http://localhost:5173)
npm run dev

# Production build
npm run build

# Lint
npm run lint
```
