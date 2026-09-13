---
name: boardsync-review
description: Review user-written Boardsync code with concrete findings, verification limits, and a focused ownership checkpoint; use for review requests rather than automatic fixes.
---
# Review for ownership
- Inspect the requested diff, callers, nearest working slice, and relevant migrations/types. Preserve unfinished user work.
- Trace input to output. Backend: names/binding, DI/validators, parameterization/mapping, resource scope, affected rows, status. Frontend: response contract, query keys/invalidation, loading/errors, installed-version docs.
- Ordering: inspect empty lists, endpoints, invalid neighbors, parent boundaries, equal positions, and concurrency when relevant.
- Rank concrete findings by impact. Give file/line evidence, triggering scenario, consequence, and minimal correction. Distinguish planned features from regressions.
- Run available relevant checks; explain what needs HTTP/database verification. Compilation alone does not prove correctness.
- A review does not authorize silent application edits. Give a focused correction task and one question about its reason. If no defects are found, state the verification limit.
- Record understanding only after the user's explanation; avoid repeating demonstrated lessons.
