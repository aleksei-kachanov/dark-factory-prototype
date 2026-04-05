# Pipeline Telemetry

Appended automatically by the telemetry-agent at the end of each pipeline run (when `review-ready` label is applied).

## Legend

| Column | Description |
|--------|-------------|
| Issue | GitHub issue number |
| Date | UTC date of pipeline completion |
| Plan tests specified | Test cases listed in issue-agent's DoD |
| Plan tests skipped | Tests from plan NOT written by testing-agent (Pass 1 DoD) |
| Critic rounds | Number of critic challenge rounds before sign-off (1 = first-pass clean) |
| Backend dev iterations | Number of `dotnet test` runs before backend developer DoD (1 = first-pass green) |
| Frontend dev iterations | Number of `npm test` runs before frontend developer DoD (1 = first-pass green) |
| Supplementary tests | Tests added by testing-agent Pass 2 beyond the plan |
| Reviewer verdict | `APPROVED` or `CHANGES REQUESTED` |
| Reviewer findings fixed | Count of BLOCKER+FIX-REQUIRED findings resolved before PR |
| Notes | Any anomalies or observations |

## Runs

| Issue | Date | Plan tests specified | Plan tests skipped | Critic rounds | Backend dev iter | Frontend dev iter | Supplementary tests | Reviewer verdict | Reviewer findings fixed | Notes |
|-------|------|---------------------|--------------------|---------------|-----------------|-------------------|---------------------|-----------------|------------------------|-------|
