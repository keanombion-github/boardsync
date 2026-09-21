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

User Postman evidence (2026-09-14): CreateColumn To Do returned 201 Created, success true, ID 0027d34e-ddaf-4555-9648-70980174b1bc for board 51854fe9-0a9c-412d-87a5-888520ba7b4d. Board-detail readback and In Progress column remain unconfirmed.

User board-detail evidence (2026-09-14): To Do 0027d34e-ddaf-4555-9648-70980174b1bc at position 1; In Progress 1a3d624b-e630-494f-a4b4-94b2caeb0f53 at position 2. Both returned under board 51854fe9-0a9c-412d-87a5-888520ba7b4d. Next: create a card and verify it in SQL; board-detail DTO does not yet include cards.

User Postman evidence (2026-09-14): CreateCard returned 201 Created, success true, ID a68d8b18-7f16-49b3-8315-1fec6be52dcd in To Do column 0027d34e-ddaf-4555-9648-70980174b1bc. SQL readback remains unconfirmed.

User SQL readback confirmed (2026-09-14): card a68d8b18-7f16-49b3-8315-1fec6be52dcd persisted in To Do at position 1 with requested title/description. Next checkpoint: UpdateCard and SQL readback, checking column_id/position remain unchanged.

User SQL evidence (2026-09-14): UpdateCard changed title/description for a68d8b18-7f16-49b3-8315-1fec6be52dcd; To Do column and position 1 unchanged. Next: reproduce empty-destination MoveCard rejection, then guided validation/position fix.

User SQL evidence (2026-09-14): card a68d8b18-7f16-49b3-8315-1fec6be52dcd moved to In Progress 1a3d624b-e630-494f-a4b4-94b2caeb0f53 at position 1, content preserved. Null-neighbor fallback verified for empty destination. Next: determine whether target contains other cards before permitting null-neighbor fallback; concurrency remains unresolved.

Static review (2026-09-14): MoveCard now checks for other destination cards only when both neighbors are absent, returns NeighborsRequired before updating, and maps Moved/CardNotFound/NeighborsRequired to 200/404/400. Backend build passed with zero warnings/errors. Runtime verification of the new guard remains pending. Missing-card error precedence, destination scope/validity, neighbor validity, and concurrent ordering remain unresolved.

User runtime evidence (2026-09-16): GetBoardById returned one ordered To Do column with two ordered cards and one In Progress column with cards: []. Reviewed handler uses one LEFT JOIN query, groups flat rows by column ID, and filters nullable card rows inside each group. A repeat build was blocked only because the running API locked Boardsync.Api.exe; the live response confirms the changed source was running.

User browser evidence (2026-09-18): Next.js dynamic route /boards/[boardId] resolves the URL board ID through the Next.js 16 async params contract and renders it. Next checkpoint: pass boardId into a client component and fetch BoardDetail with TanStack Query.

User browser evidence (2026-09-18): BoardView fetches BoardDetail with TanStack Query and renders the board name, ordered columns, nested cards, and an empty-column message. TypeScript check passed. Visual direction for later polish: dark glassmorphism with theme switching/options; current priority remains feature and component foundations.

Frontend review (2026-09-21): Create-card UI now uses a per-column Base UI modal opened by a plus trigger. The form owns mutation state, invalidates the exact board query, and closes through onCreated after the refetch completes. TypeScript, ESLint, and the Next.js production build pass. User reports the modal appearance/interaction is improved; a successful browser-to-database create through the modal remains to be explicitly confirmed.

User runtime evidence (2026-09-21): created two cards through the per-column modal. POST, database persistence, exact-board cache invalidation/refetch, modal close, and updated board rendering are confirmed end to end.

User runtime evidence (2026-09-21): the per-card delete alert was tested through Cancel and Confirm. DELETE returned the expected success behavior, the exact board query refetched, the card disappeared immediately, and a browser refresh confirmed the deletion persisted. Frontend lint and production build pass. Next checkpoint: add the MoveCard API client before wiring dnd-kit interactions.

Static review (2026-09-21): the frontend MoveCard client now matches `PUT /api/cards/{id}/move`, sends the destination column and nullable neighbor positions, parses the standard API wrapper, and surfaces backend errors. Frontend lint and production build pass. Runtime movement through this client remains unverified; next checkpoint is local dnd-kit identity wiring before issuing mutations.

Product direction captured (2026-09-21): evolve Boardsync toward a Jira-style dashboard with user authentication, board creation/navigation, ticket assignment, comments, and image attachments. Sequence these after a deployable core board release so authentication/ownership precedes user assignment and storage design precedes uploads.
