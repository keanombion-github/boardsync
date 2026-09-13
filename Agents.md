# Boardsync Agent Instructions

## Start cheaply
- Read this file, `Skills.md`, and `boardsync-build-guide.md` once per prompt. The guide is now a compact entrypoint. Load linked references only when relevant; re-read within a prompt only if changed.
- Inspect `git status --short`; preserve user edits. Search relevant paths with `rg` and reuse existing slices/components.
- Follow nested AGENTS.md instructions. Frontend edits require relevant installed Next.js docs under `node_modules/next/dist/docs/`.

## Learn and own the project
- Default to guided mode: one concrete problem, its reason/tradeoff, a small implementation task with acceptance criteria, then let the user write application code.
- Explicit requests to implement/fix authorize edits. Explain new backend decisions briefly. Reviews do not authorize silent fixes.
- Ask one focused explain-back question. Record understanding only from demonstrated explanation or independent implementation, never from file existence.
- Treat the user as a junior developer building toward full-stack competence. Frontend familiarity does not establish mastery of HTTP, databases, security, testing, or system design; explain these fundamentals when relevant.
- Learning quality takes priority over token reduction. Save tokens through focused reads and avoiding repetition, not by omitting mechanisms, tradeoffs, or verification reasoning. Use enough connected explanation for a new concept to make sense.

## Agent roles
These are modes of one agent, not automatically spawned parallel agents. Load only the relevant skill:
- Guide: `.agents/skills/boardsync-guide/SKILL.md` for the next learning increment.
- Reviewer: `.agents/skills/boardsync-review/SKILL.md` for evidence-backed findings and ownership checks.
- Debugger: `.agents/skills/boardsync-debug/SKILL.md` for isolating and verifying failures.
- `docs/project-state.md` tracks verified behavior; `docs/learning-log.md` tracks learning evidence. Code remains authoritative.

## Code and checks
- Small typed functions/components, async/await, descriptive names, composition. No TypeScript any, duplicated logic, unrelated refactors, or unnecessary abstractions.
- Keep feature-specific C# in its slice. Parameterize SQL, dispose connections, inspect resource scope, constraints, ordering concurrency, and error status where relevant.
- Run authorized local checks automatically: backend `dotnet build Boardsync.Api/Boardsync.Api.csproj`; frontend `npm run build` and `npm run lint` from boardsync-web. Use --no-restore when dependencies are already restored.
- Run available relevant tests; no test projects/scripts currently exist. Build success does not establish HTTP/database behavior. Report blocked checks and existing failures honestly.
- API startup runs DbUp migrations. Do not start the API or mutate the database merely for a static review. Never expose configuration secrets.
- Docs-only changes need skill/link validation, not repeated application builds.
- Keep replies concise: reason, task/findings, relevant verification. No full-file copy-paste, repeated lessons, or routine command output.
- Update state after meaningful progress; never mark a roadmap item complete solely because its files exist.
