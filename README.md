# DarkFactory.Weather

A 5-day weather forecast API and UI — built as a prototype for a **fully automated, AI-driven SDLC pipeline** running entirely on GitHub Issues and GitHub Actions.

The application itself is intentionally simple: an ASP.NET Core 8 Web API serving regional weather forecasts, consumed by a React 19 + TypeScript frontend. The point is the pipeline that builds it.

---

## What This Demonstrates

Every feature in this repository was built by AI agents — no human wrote production code. Each GitHub Issue travels through an 11-stage pipeline:

1. **Triage & Planning** → implementation plan with acceptance criteria and TDD test cases
2. **Critic Review** → plan challenged before a single line of code is written
3. **UX Design** → UX spec produced (component states, flows, accessibility) before tests are written
4. **Failing Tests** → backend (xUnit) and frontend (Vitest) tests written first, state-coverage tests from UX spec included
5. **Implementation** → backend then frontend code written to make tests green
6. **Blind Code Review** → structural quality check using diff only (no spec context)
7. **AC Verification** → every acceptance criterion verified against code
8. **UX Review** → five-state component audit against UX spec
9. **Coverage Review** → supplementary tests added if gaps found
10. **PR Coordination** → single go/no-go decision, PR opened or findings routed back by layer
11. **Telemetry** → pipeline metrics recorded for health analysis

The pipeline is **label-driven**: each stage completes by applying a GitHub label that triggers the next workflow. No human intervention is required between `plan-ready` and `review-ready`.

---

## SDLC Pipeline

### Label State Machine

```
issue opened
     │
     ▼
[issue-agent]──────────────────────────────► rejected
     │                                        needs-clarification
     ├──► plan-ready
     │         │
     │    [critic-agent] ──────────────────► plan-challenged ──► (author fixes) ──► plan-ready
     │         │                                                              └──► needs-clarification (round 3)
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
     │    └── Step 2: frontend testing agent (writes failing Vitest tests → adds label)
     │         │
     │         ▼
     │      tests-ready
     │         │
     │    [developer-agent]
     │    ├── Step 1: backend developer  (implements ASP.NET Core 8 code)
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
     │         ├──► tests-ready (ROUTE_BACK — findings sent back by layer)
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

15 agent files. `testing-backend` and `testing-frontend` each run in two passes (rows 4/5 and 11/12 below are the same files invoked with different context).

| # | Agent | Trigger | Responsibility |
|---|-------|---------|----------------|
| 1 | **issue-agent** | issue opened/edited | Triages issue, routes to TDD or DevOps path, generates implementation plan with ACs and TDD test cases |
| 2 | **critic-agent** | label `plan-ready` | Challenges the plan on 6 axes before code is written; max 3 rounds before human escalation |
| 3 | **ux-designer-agent** | label `planned` | Produces `docs/ux/<N>.md` — component state grid, user flows, accessibility, data contract; or skips if no UI changes |
| 4 | **testing-backend-agent** | label `ux-ready` (step 1) | Creates feature branch, writes failing xUnit tests, confirms red phase |
| 5 | **testing-frontend-agent** | label `ux-ready` (step 2) | Writes failing Vitest tests including state-coverage tests from UX spec; confirms red phase; applies `tests-ready` |
| 6 | **developer-backend-agent** | label `tests-ready` (step 1) | Implements ASP.NET Core 8 production code; iterates until all backend tests pass (3-strikes rule) |
| 7 | **developer-frontend-agent** | label `tests-ready` (step 2) | Implements React 19 UI; reads backend DoD for DTO changes; validates tsc + vite build |
| 8 | **reviewer-agent** | label `implementation-done` (gate 1) | Blind structural review — receives diff only, checks architecture, security, TS↔C# DTO sync |
| 9 | **po-verifier-agent** | label `implementation-done` (gate 2) | Maps every AC to implementation + test; dynamic spec-to-code trace per plan-described endpoints |
| 10 | **ux-reviewer-agent** | label `implementation-done` (gate 3) | Five-state component audit against UX spec; interaction patterns; accessibility; null guard check |
| 11 | **testing-backend-agent** (P2) | label `implementation-done` (gate 4) | Reviews xUnit coverage gaps, adds supplementary tests |
| 12 | **testing-frontend-agent** (P2) | label `implementation-done` (gate 5) | Reviews Vitest coverage gaps, adds supplementary tests |
| 13 | **pr-coordinator-agent** | label `implementation-done` (gate 6) | Reads all 5 gate verdicts; routes findings by layer (backend/frontend/UX) or opens PR |
| 14 | **devops-agent** | label `devops` | Implements CI/CD, Dockerfiles, IaC; validates build + tests; opens PR |
| 15 | **telemetry-agent** | label `review-ready` | Appends metrics row to `docs/pipeline/telemetry.md`; writes daily JSON snapshot |
| 16 | **pipeline-analyst-agent** | Monday 09:00 UTC or `/pipeline-analysis` | Reads telemetry, computes per-agent health metrics, suggests one concrete improvement |
| 17 | **pipeline-audit-agent** | Daily 09:00 UTC or manual | Audits last 24 h of runs; reports blocked pipelines, high iterations, stalls as GitHub Discussion |

### Key Design Principles

**Information asymmetry** — agents receive only the inputs they need. The blind reviewer sees the diff, not the spec. The critic reads the plan, not the issue body. This prevents rationalization and forces each agent to evaluate on its own merits.

**Scope constraints** — every agent that writes code has an explicit scope constraint in its instructions. Developer agents write only to their respective layer; testing agents write only test files. Scope is enforced by instruction.

**TDD ordering** — tests are written and confirmed failing *before* the developer agents run. The `tests-ready` label is applied only after both backend and frontend testing agents have committed and verified their red phase.

**Layer-precise routing** — when `pr-coordinator` routes back, it separates backend findings (files under `DarkFactory.Weather/`) from frontend findings (files under `dark-factory-ui/`) and addresses each developer agent independently.

**Telemetry + observability** — every agent emits a structured `📊 TELEMETRY` JSON block. The telemetry agent aggregates these into `docs/pipeline/telemetry.md` and `docs/pipeline/metrics/daily/`. The audit agent monitors for stalls, high iteration counts, and blocked pipelines.

---

## Project Structure

```
.
├── DarkFactory.Weather/              — ASP.NET Core 8 Web API
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
│   ├── agents/                       — 15 agent definition files (.agent.md)
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
| Backend | ASP.NET Core 8, C# 12, xUnit, Moq |
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
