---
name: boardsync-debug
description: Diagnose a Boardsync build, HTTP, SQL, or frontend data-flow failure with a small evidence-driven investigation and learning-oriented fix.
---
# Debug one failure
- Establish expected/actual behavior and the first useful error. Use existing logs/local checks before requesting information. Never print secrets or connection strings.
- Locate the boundary: build -> startup/DI/routing -> binding/validation -> SQL/constraints/mapping -> serialization -> fetch/cache/render.
- Form one hypothesis and run the smallest discriminating check. Stop repeating checks once evidence resolves it; avoid broad dumps and speculative rewrites.
- Startup runs DbUp migrations. Prefer build/static inspection first; API/database mutations require the user's requested development scope.
- Explain the root cause using the actual input and file. Guided mode gives a minimal edit task; an explicit fix request authorizes edits.
- Verify the original failure and a relevant nearby edge case. Distinguish compiler checks from HTTP/database checks. If no harness exists, give a concrete manual check rather than inventing passing tests.
- Record resolved behavior only after verification; keep unfinished checks visible. Capture a reusable lesson after demonstrated understanding.
