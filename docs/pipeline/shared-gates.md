# Shared Agent Gates

Canonical definitions for cross-cutting protocols used by all DarkFactory pipeline agents.
Each agent references this document rather than inlining these rules.
**Update here once; all agents pick up the change automatically.**

---

## Project Layout

The canonical layout of the repository. Agents reference this section instead of
inlining their own copy.

```
Repository root
├── DarkFactory.slnx                      — solution file
├── DarkFactory.Weather/                  — backend (ASP.NET Core 8 Web API)
│   ├── Controllers/WeatherController.cs
│   ├── Services/IWeatherService.cs
│   ├── Services/WeatherService.cs
│   ├── Models/WeatherForecast.cs
│   ├── Dtos/WeatherForecastDto.cs
│   └── Program.cs                        — DI wiring
├── DarkFactory.Weather.Tests/            — backend tests (xUnit + Moq)
│   ├── WeatherServiceTests.cs
│   └── WeatherControllerTests.cs
├── dark-factory-ui/                      — frontend (React 19 + TypeScript + Vite)
│   ├── src/
│   │   ├── App.tsx
│   │   ├── components/
│   │   │   ├── RegionSelector.tsx
│   │   │   └── ForecastTable.tsx
│   │   ├── types/weather.ts              — WeatherForecastDto, Region, REGIONS
│   │   └── test-setup.ts                 — @testing-library/jest-dom setup
│   ├── vite.config.ts
│   ├── vitest.config.ts
│   └── package.json
└── docs/pipeline/                        — pipeline documentation and state
    ├── shared-gates.md                   — this file
    ├── telemetry.md
    ├── workflow-state/<issue>.json
    └── metrics/
```

**Build and test commands:**

| Target | Command |
|---|---|
| Backend build | `dotnet build DarkFactory.slnx` |
| Backend tests | `dotnet test DarkFactory.slnx` |
| Frontend build (tsc + vite) | `cd dark-factory-ui && npm run build` |
| Frontend tests (Vitest) | `cd dark-factory-ui && npm test` |
| Frontend lint | `cd dark-factory-ui && npm run lint` |

---

## Verbose Reasoning Protocol

Every agent must emit visible reasoning traces at each decision point.

**Format:** `> 🔍 [AGENT_NAME] TYPE: message`

```
> 🔍 [AGENT] STEP: <what I'm doing now>
> 🔍 [AGENT] INPUT: <what I read / received>
> 🔍 [AGENT] DECISION: <what I decided and why>
> 🔍 [AGENT] GATE: <gate name> — PASS | FAIL — <evidence>
> 🔍 [AGENT] HALT: <why I stopped>
```

Rules:
- Emit `STEP` before starting any workflow step.
- Emit `DECISION` after every classification or routing choice.
- Emit `GATE` for every shared gate check.
- Emit `HALT` when stopping early.
- Never skip traces — silent decisions are invisible decisions.

---

## Telemetry Block Protocol

Every agent must emit a structured JSON telemetry block as the **last output** of
every invocation — even when halted.

**Format:**

```
📊 TELEMETRY
```json
{
  "agent": "<agent-name>",
  "issue": <N>,
  "stage": "<see Agent Telemetry Quick-Ref below>",
  "verdict": "<see Agent Telemetry Quick-Ref below>",
  "gates": {
    "passed": ["<gate-name>"],
    "failed": ["<gate-name>"]
  },
  "findings": { "critical": 0, "high": 0, "low": 0 },
  "rework": <true if retry invocation>,
  "halted": <true if stopped early>,
  "halt_reason": "<reason or null>"
}
```
```

Rules:
- Emit exactly once, at the very end of every invocation.
- `rework: true` when this is a re-run after a prior blocked/failed attempt.
- `halted: true` + `halt_reason` when the agent stopped before completing.
- `findings` counts are 0 for agents that do not produce findings.

**Agent Telemetry Quick-Ref** — each agent's `stage` and valid `verdict` values:

| Agent | `stage` (Pass 1) | `stage` (Pass 2) | Valid `verdict` values |
|---|---|---|---|
| issue-agent | `"triage"` (reject/clarify) / `"spec"` (plan posted) | — | `"ROUTED"` / `"HALTED"` |
| critic-agent | `"critic"` | — | `"SIGN-OFF"` / `"CHALLENGE"` |
| testing-backend-agent | `"test-backend-pass1"` | `"test-backend-pass2"` | `"PASS"` / `"FAIL"` |
| testing-frontend-agent | `"test-frontend-pass1"` | `"test-frontend-pass2"` | `"PASS"` / `"FAIL"` |
| developer-backend-agent | `"implement-backend"` | — | `"PASS"` / `"HALTED"` |
| developer-frontend-agent | `"implement-frontend"` | — | `"PASS"` / `"HALTED"` |
| reviewer-agent | `"review"` | — | `"APPROVED"` / `"CHANGES_REQUESTED"` |
| po-verifier-agent | `"po-verify"` | — | `"PO_ACCEPTED"` / `"PO_REJECTED"` |
| devops-agent | `"devops"` | — | `"PASS"` / `"HALTED"` |
| telemetry-agent | `"telemetry"` | — | `"PASS"` |
| pipeline-analyst | `"analysis"` | — | `"PASS"` |
| pipeline-audit | `"audit"` | — | `"PASS"` |

---

## Blind Review Protocol

Used when reviewing a diff or PR without reading the linked spec or issue.

- Do NOT open the issue body, implementation plan, or any context beyond the diff.
- Evaluate only: the diff and the existing source files it touches.
- Goal: surface implementation drift and boundary violations that spec-aware review misses.
- After the blind review is complete, you may read the spec to reconcile findings.

---

## Minimum-3-Findings Threshold

Any review, validation, or diagnostic report must produce one of:
- **≥ 3 distinct findings** with evidence, OR
- An explicit **Clean Sweep** confirming what was checked and why 0 real issues were found.

Clean Sweep format:
```
## Clean Sweep
**Categories checked:** [list]
**Evidence reviewed:** [what was read]
**Findings:** 0 real issues found.
```

---

## Premise Verification Protocol

Required before proposing any bug fix.

1. Read the actual source file at the line referenced in the failure.
2. Confirm the stated root cause is present in the code as-written.
3. If the premise does not hold, emit a **False-Premise Finding** instead of a fix:

```
False Premise:    <what was claimed>
Reality:          <what the code actually does at file:line>
Recommended action: re-diagnose | close as not-a-bug
```

---

## Discovery Report Protocol

Emitted by any agent when a finding changes the scope of the active issue.
**Halt all work** until the Discovery Report is reviewed by a human.

```
## Discovery Report — #<issue-number>

**Found:** <what was discovered>
**Scope impact:** <why this changes the original issue>
**Options:**
  A. <path> — effort: <S/M/L>
  B. <path> — effort: <S/M/L>

Human review required before proceeding.
```

After posting, add label `needs-clarification` and stop. Do not commit, do not open a PR.

---

## Workflow State Protocol

Every agent that advances the pipeline stage must update
`docs/pipeline/workflow-state/<issue-number>.json` and commit it.

**Always commit the state file together with (or immediately after) the change that
advances the stage. Never leave the state file uncommitted.**

Schema:
```json
{
  "issue": <N>,
  "title": "<issue title>",
  "stage": "<current stage>",
  "updated": "<ISO date>",
  "critic_rounds": <N>,
  "critic_sign_off": "<date or null>",
  "developer_backend_iterations": <N>,
  "developer_frontend_iterations": <N>,
  "reviewer_verdict": "<APPROVED|CHANGES_REQUESTED|null>",
  "pr_number": <N or null>
}
```

Agents write only their own fields. Never overwrite fields owned by other agents.

| Field | Owner |
|---|---|
| `stage` | whichever agent last advanced the pipeline |
| `critic_rounds`, `critic_sign_off` | critic-agent |
| `developer_backend_iterations` | developer-backend-agent |
| `developer_frontend_iterations` | developer-frontend-agent |
| `reviewer_verdict` | reviewer-agent (via testing-frontend-agent Pass 2) |
| `pr_number` | testing-frontend-agent Pass 2 |
