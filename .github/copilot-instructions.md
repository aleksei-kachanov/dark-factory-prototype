# DarkFactory.Weather — Copilot Instructions

## Project Layout

```
DarkFactory.Weather/          — ASP.NET Core net10.0 Web API
  Controllers/WeatherController.cs
  Services/IWeatherService.cs + WeatherService.cs
  Services/IAustinWeatherService.cs + AustinWeatherService.cs
  Models/WeatherForecast.cs
  Dtos/WeatherForecastDto.cs    — includes windDirection
  Program.cs                   — DI wiring

DarkFactory.Weather.Tests/    — xUnit + Moq
dark-factory-ui/              — React 19 + TypeScript + Vite
  src/App.tsx
  src/components/RegionSelector.tsx, ForecastTable.tsx
  src/types/weather.ts          — WeatherForecastDto, Region, REGIONS

DarkFactory.slnx              — solution file
docs/pipeline/                — SDLC pipeline documentation and state
  shared-gates.md             — cross-cutting protocols (ALL agents read this)
  sdlc-pipeline.md            — pipeline architecture
  workflow-state/<N>.json     — per-issue pipeline state
```

## Validation Commands

Run before every commit:

```bash
dotnet test DarkFactory.slnx
cd dark-factory-ui && npm test -- --run
```

## SDLC Pipeline — Entry Point

**Single command to start the full pipeline:**

```
@issue-agent implement issue #<N>
```

The issue agent reads the issue, produces a WHAT spec, applies `spec-ready`,
and invokes the next agent. Each agent invokes the next via its **Pipeline Handoff**
section. The full chain runs within the Copilot Chat session.

**Full pipeline sequence:**
```
@issue-agent → @architect-agent → @critic-agent ↔ (loop up to 3×) ↔ @architect-agent
  → @ux-designer-agent
  → @testing-backend-agent (Pass 1) + @testing-frontend-agent (Pass 1)
  → @developer-backend-agent → @developer-frontend-agent
  → @reviewer-agent + @po-verifier-agent + @ux-reviewer-agent
    + @testing-backend-agent (Pass 2) + @testing-frontend-agent (Pass 2)
  → @pr-coordinator-agent
  → @telemetry-agent
```

If PR coordinator routes to fix: `@fixer-agent` → re-runs Pass 2 (max 5 iterations).

## Branch Convention

- Feature branches: `feature/issue-<N>` — created by ux-designer-agent
- Never commit to `main` directly
- PRs target `enrich_agents`

## Worktree Convention (Parallel Issue Isolation)

Each issue runs in its own isolated git worktree. This allows two issues to progress
simultaneously without branch-switching conflicts.

```bash
# Created by ux-designer-agent at pipeline start
git worktree add .worktrees/issue-<N> -b feature/issue-<N>

# All write agents work inside the worktree
cd .worktrees/issue-<N>

# Removed by telemetry-agent after pipeline completes
git worktree remove .worktrees/issue-<N> --force
```

**Hard rule**: never run parallel agents in the same worktree.
See `docs/pipeline/shared-gates.md` — Parallel Work Isolation Rule.

## Hard Constraints

- **Critic** reads only the Technical Design comment — NOT the issue body (information asymmetry by design)
- **Reviewer** reads only the git diff — NOT the issue or plan
- **Backend tests must be green** before frontend developer starts
- **Developer max 3 retries** per side — apply `needs-clarification` on 3rd failure
- **External HTTP calls**: `IHttpClientFactory` only (never `new HttpClient()`)
- **Path scoping**: backend agents touch `DarkFactory.Weather/` only; frontend agents touch `dark-factory-ui/` only; testing agents touch `*.Tests/` or `*.test.*` only

## Protocols

See `docs/pipeline/shared-gates.md` for:
- Verbose Reasoning Protocol (required trace lines)
- Telemetry Block Protocol (📊 TELEMETRY JSON at end of each agent comment)
- Verification Gate Protocol
- Workflow State Protocol (`docs/pipeline/workflow-state/<N>.json`)

## TypeScript Contract

```typescript
export interface WeatherForecastDto {
  date: string
  temperatureC: number
  temperatureF: number
  summary: string | null
  humidity: number
  windSpeed: number
  windDirection: string
}
export type Region = 'tropical' | 'arid' | 'temperate' | 'continental' | 'polar' | 'austin'
export const REGIONS: Region[] = ['tropical', 'arid', 'temperate', 'continental', 'polar', 'austin']
```
