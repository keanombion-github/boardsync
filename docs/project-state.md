# Verified Project State

Reviewed 2026-09-13. Phase 2a, core CRUD in progress.

## Implemented source
- Backend targets net9.0, rather than the planned .NET 8.
- Program.cs maps CreateBoard, GetBoards, GetBoardById; CreateColumn, DeleteColumn, ReorderColumn; CreateCard, UpdateCard, DeleteCard, MoveCard.
- DbUp embeds migrations 001-004 for users, boards, columns, cards; API startup runs migrations.
- GetBoardById returns a board and ordered columns, without cards.
- Frontend: Next.js 16.2.10, React 19.2.4, TanStack Query provider and board listing. dnd-kit/Zustand installed; installation alone does not prove usage.
- No board detail route, auth slices, test projects, frontend test script, or CI workflow found in reviewed inventory.

## Known issues / learning tasks
1. Resolved in source: user renamed MoveCardEndpoint to WithName("MoveCard"). All 10 literal endpoint names are distinct; backend build passed. User Postman screenshot confirms MoveCard returns 400 VALIDATION_ERROR with both neighbor positions null.
2. MoveCard/ReorderColumn reject neither-neighbor input, preventing moves into empty destinations. Define empty/top/middle/bottom behavior.
3. Position calculation is duplicated. Creation reads MAX then inserts separately; concurrent writes can produce equal positions. Discuss ordering invariants before fixing.
4. MoveCard updates by card ID and target column without same-board or authorization checks. Auth is planned; current API is not production-secure.
5. Frontend app/page.tsx still uses PASTE-YOUR-USER-UUID-HERE. Build success cannot establish the advertised end-to-end flow.
6. Board detail needs cards with correct grouping and without N+1 queries.

## Verification
- Backend dotnet build --no-restore passed with zero warnings/errors.
- Frontend checks recorded below when finished.
- No live API/database verification. Runtime behavior and applied migrations remain unverified in this review.

## Resume checkpoint
Next guided task: preserve BadHttpRequestException.StatusCode in global middleware with a safe response. Retest missing-body and validation cases before real card writes.

Review validation: frontend lint and production build passed (build retried with font network access). Skill frontmatter/names/entrypoint paths checked locally; bundled quick_validate.py could not run because PyYAML is missing. No application source was changed by this review.


Runtime evidence (user Postman, 2026-09-13): missing body throws BadHttpRequestException, currently mapped to 500 by middleware; supplying JSON with null neighbors returns expected 400 VALIDATION_ERROR. Database writes remain unverified.

User-confirmed runtime checks (2026-09-13): after middleware correction, missing-body request returns 400 INVALID_REQUEST and supplied JSON with null neighbors returns 400 VALIDATION_ERROR. Next checkpoint: create a board using an existing users.id and read it back via GetBoards. Real card writes remain unverified.

User runtime evidence: GetBoards screenshot shows 200 OK, success true, board 51854fe9-0a9c-412d-87a5-888520ba7b4d named My First Kanban Board. User reports prior creation test; POST status not independently shown. Next smoke check: CreateColumn then GetBoardById.
