# DarkFactory.Weather

A 5-day weather forecast API and UI — built as a prototype for a **fully automated, AI-driven SDLC pipeline** running entirely on GitHub Issues and GitHub Actions.

The application itself is intentionally simple: a .NET 10 (ASP.NET Core) Web API serving regional weather forecasts, consumed by a React 19 + TypeScript frontend. The point is the pipeline that builds it.

---

## What This Demonstrates

Every feature in this repository was built by AI agents — no human wrote production code. Each GitHub Issue travels through a **12-stage** pipeline:

1. **Triage & WHAT spec** → structured requirements and acceptance scenarios; adds `spec-ready` on the TDD path
2. **Technical design** → architect-agent turns WHAT into HOW, adds `plan-ready` for the critic
3. **Critic Review** → plan challenged before a single line of code is written; `plan-challenged` returns to the architect (not straight to code)
4. **UX Design** → UX spec produced (component states, flows, accessibility) before tests are written
5. **Failing Tests** → backend (xUnit) and frontend (Vitest) tests written first, state-coverage tests from UX spec included
6. **Implementation** → backend then frontend code written to make tests green
7. **Blind Code Review** → structural quality check using diff only (no spec context)
8. **AC Verification** → every acceptance criterion verified against code
9. **UX Review** → five-state component audit against UX spec
10. **Coverage Review** → supplementary tests added if gaps found
11. **PR Coordination** → single go/no-go decision: open PR or apply `fix-ready` for the fixer-agent (targeted fixes, then Pass 2 re-runs)
12. **Telemetry** → pipeline metrics recorded for health analysis

The pipeline is **label-driven**: each stage completes by applying a GitHub label that triggers the next workflow (chained by `.github/actions/dispatch-next-stage` after each run). No human intervention is required on the happy path from `spec-ready` through `review-ready`.

---

## SDLC Pipeline

### Label State Machine

```
issue opened
     │
     ▼
[issue-agent]──────────────────────────────► rejected
     │                                        needs-clarification
     ├──► spec-ready (TDD — WHAT spec)
     │         │
     │    [architect-agent] ───────────────► plan-ready (HOW / technical design)
     │         ▲                              │
     │         └── plan-challenged ───────────┘ (critic findings → architect revises)
     │         │
     │    [critic-agent] ──────────────────► plan-challenged ──► needs-clarification (round 3)
     │         │
     │         ▼
     │       planned
     │         │
     │    [ux-designer-agent]
     │    ├── if UI changes: writes docs/ux/<N>.md (state grid, flows, a11y)
     │    └── if no UI changes: skips spec, posts UX_SKIPPED
     │         │
     │         ▼
     │       ux-ready
     │         │
     │    [testing-agent Pass 1]
     │    ├── Step 1: backend testing agent  (writes failing xUnit tests)
     │    └── Step 2: frontend testing agent (writes failing Vitest tests)
     │    └── workflow removes ux-ready, adds tests-ready (after both steps)
     │         │
     │         ▼
     │      tests-ready
     │         │
     │    [developer-agent]
     │    ├── Step 1: backend developer  (implements API code)
     │    └── Step 2: frontend developer (implements React 19 UI, syncs DTOs)
     │         │
     │         ▼
     │    implementation-done
     │         │
     │    [testing-agent Pass 2 — 6 sequential gates]
     │    ├── Gate 1: blind reviewer       (structural code quality, diff only)
     │    ├── Gate 2: PO verifier          (plan compliance, AC verification)
     │    ├── Gate 3: UX reviewer          (five-state audit against docs/ux/<N>.md)
     │    ├── Gate 4: backend testing P2   (xUnit coverage, supplementary tests)
     │    ├── Gate 5: frontend testing P2  (Vitest coverage, supplementary tests)
     │    └── Gate 6: PR coordinator       (aggregates verdicts → OPEN_PR or ROUTE_BACK)
     │         │
     │         ├──► fix-ready (ROUTE_BACK — fixer-agent, then back to implementation-done → Pass 2)
     │         ▼
     │      review-ready ◄─── devops-agent (parallel path for infra/CI issues)
     │         │
     │    [telemetry-agent] (records pipeline run metrics)
     │         │
     │         ▼
     │      PR merged → deploy.yml (build → test → publish → Azure Functions)
     │
     └──► devops ──► [devops-agent] ──► review-ready
```

### Agents

**17** agent definition files under `.github/agents/`. The same **testing-backend** and **testing-frontend** files run twice (Pass 1 / Pass 2) with different prompts. Orchestration is in `.github/workflows/*.yml`. Most workflows end with `.github/actions/dispatch-next-stage`, which runs the next workflow from the issue’s current labels; **`telemetry-agent.yml` does not** (it only records metrics after `review-ready`).

| # | Agent | Trigger | Responsibility |
|---|-------|---------|----------------|
| 1 | **issue-agent** | issue opened/edited (see workflow guard) | WHAT spec + `spec-ready`, or DevOps plan + `devops`, or halt |
| 2 | **architect-agent** | `spec-ready` / `plan-challenged` | Technical design → `plan-ready` |
| 3 | **critic-agent** | `plan-ready` | `planned` or `plan-challenged` (max 3 rounds) |
| 4 | **ux-designer-agent** | `planned` | `docs/ux/<N>.md`, `ux-ready` |
| 5 | **testing-backend-agent** | `ux-ready` / `implementation-done` | Pass 1: failing xUnit; Pass 2: coverage |
| 6 | **testing-frontend-agent** | `ux-ready` / `implementation-done` | Pass 1: failing Vitest; Pass 2: coverage |
| 7 | **developer-backend-agent** | `tests-ready` (step 1) | Backend implementation |
| 8 | **developer-frontend-agent** | `tests-ready` (step 2) | Frontend implementation, `implementation-done` |
| 9 | **reviewer-agent** | `implementation-done` (Pass 2) | Blind diff review |
| 10 | **po-verifier-agent** | `implementation-done` | AC trace |
| 11 | **ux-reviewer-agent** | `implementation-done` | UX audit |
| 12 | **pr-coordinator-agent** | `implementation-done` | `OPEN_PR` / `ROUTE_BACK` (`fix-ready`) |
| 13 | **fixer-agent** | `fix-ready` | Targeted fixes → `implementation-done` |
| 14 | **devops-agent** | `devops` | Infra/CI → `review-ready` |
| 15 | **telemetry-agent** | `review-ready` | Telemetry files |
| 16 | **pipeline-analyst-agent** | schedule / `/pipeline-analysis` | Health report |
| 17 | **pipeline-audit-agent** | schedule / manual | Discussion audit |

### Key Design Principles

**Information asymmetry** — agents receive only the inputs they need. The blind reviewer sees the diff, not the spec. The critic reads the plan, not the issue body. This prevents rationalization and forces each agent to evaluate on its own merits.

**Scope constraints** — every agent that writes code has an explicit scope constraint in its instructions. Developer agents write only to their respective layer; testing agents write only test files. Scope is enforced by instruction.

**TDD ordering** — tests are written and confirmed failing *before* the developer agents run. The workflow applies `tests-ready` (and removes `ux-ready`) only after Pass 1 backend and frontend testing steps complete.

**Layer-precise routing** — on `ROUTE_BACK`, `pr-coordinator` posts layered findings and sets `fix-ready`; **fixer-agent** applies minimal fixes, then sets `implementation-done` so Pass 2 runs again (instead of sending the issue back to full developer-agent from `tests-ready`).

**Telemetry + observability** — every agent emits a structured `📊 TELEMETRY` JSON block. The telemetry agent aggregates these into `docs/pipeline/telemetry.md` and `docs/pipeline/metrics/daily/`. The audit agent monitors for stalls, high iteration counts, and blocked pipelines.

---

## Project Structure

```
.
├── DarkFactory.Weather/              — ASP.NET Core (.NET 10) Web API
│   ├── Controllers/
│   ├── Services/
│   ├── Models/
│   ├── Dtos/
│   └── Program.cs
├── DarkFactory.Weather.Tests/        — xUnit + Moq tests
├── dark-factory-ui/                  — React 19 + TypeScript + Vite
│   └── src/
│       ├── components/               — RegionSelector, ForecastTable
│       ├── types/weather.ts          — WeatherForecastDto, Region
│       └── App.tsx
├── docs/
│   └── ux/                           — UX specs per issue (docs/ux/<N>.md)
├── .github/
│   ├── agents/                       — agent definition files (.agent.md)
│   └── workflows/                    — GitHub Actions workflows (.yml)
└── docs/pipeline/
    ├── sdlc-pipeline.md              — Full pipeline diagram + authority matrix
    ├── shared-gates.md               — Shared protocols (telemetry, workflow state, reasoning traces)
    ├── telemetry.md                  — Per-run metrics table
    └── metrics/                      — Daily JSON snapshots + latest.json
```

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 10, ASP.NET Core, xUnit, Moq |
| Frontend | React 19, TypeScript, Vite, Vitest, @testing-library/react |
| Pipeline | GitHub Actions, `actions/ai-inference@v1`, Claude Sonnet 4.6 |
| Deploy | Azure Functions, OIDC auth (no static secrets) |

---

## Local Development

**Backend**
```bash
dotnet build DarkFactory.slnx
dotnet test DarkFactory.slnx
```

**Frontend**
```bash
cd dark-factory-ui
npm ci
npm run build   # tsc + vite
npm test        # Vitest
```

---

## Pipeline Documentation

- [`docs/pipeline/sdlc-pipeline.md`](docs/pipeline/sdlc-pipeline.md) — Mermaid flowcharts, label state machine, authority matrix, execution trigger map
- [`docs/pipeline/shared-gates.md`](docs/pipeline/shared-gates.md) — Shared agent protocols: verbose reasoning, telemetry blocks, workflow state schema
- [`docs/pipeline/telemetry.md`](docs/pipeline/telemetry.md) — Historical pipeline run metrics
