# Verified Project State

Reviewed 2026-09-28. Phase 2a, core CRUD in progress.

## Implemented source
- Backend targets net9.0, rather than the planned .NET 8.
- Program.cs maps CreateBoard, GetBoards, GetBoardById; CreateColumn, DeleteColumn, ReorderColumn; CreateCard, UpdateCard, DeleteCard, MoveCard.
- DbUp embeds migrations 001-005 for users, boards, columns, cards, and query indexes; API startup runs migrations.
- GetBoardById returns a board with ordered columns and nested ordered card arrays using one joined query.
- Frontend: Next.js 16.2.10, React 19.2.4, TanStack Query, board dashboard/detail routes, card CRUD, persisted card movement, column create/delete, and column drag reordering.
- An xUnit project covers the shared fractional-position invariant. No auth slices, frontend test script, or CI workflow exist yet.

## Known issues / learning tasks
1. Repeated midpoint insertion can exhaust floating-point gaps. Define a position-normalization threshold and transaction before real-time collaboration.
2. MoveCard now resolves neighbor IDs on the server and rejects cross-board destinations, but authentication/authorization is still absent; the API is not production-secure.
3. The dashboard uses `NEXT_PUBLIC_DEMO_OWNER_ID` until authentication derives identity from verified server-side claims.
4. The updated neighbor-ID move contract, column reordering, board/column creation, and column deletion need browser/API/database runtime verification.

## Verification
- Backend dotnet build --no-restore passed with zero warnings/errors.
- The eight FractionalPosition xUnit tests pass.
- Frontend ESLint and the Next.js production build pass.
- Runtime evidence is recorded below. The 2026-09-27 additions have static build verification only; migration 005 and the new mutations have not been exercised against the live database.

## Resume checkpoint
Runtime-test the dashboard and updated ordering contract: create/open a board, create at least three columns, reorder them in both directions, move cards at top/middle/bottom and across columns, cancel one deletion, then confirm deletion and refresh. After that, begin authentication design or add API integration tests.

The next ownership checkpoint is to explain why the client sends neighbor IDs instead of positions, what the parent-row lock prevents, and why the UI refetches after a successful mutation. AI-written source is not counted as learning evidence until that explanation or an independent change demonstrates it.

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

User browser evidence (2026-09-22): dnd-kit card handles and column drop targets report the active card ID, source column ID, and destination column ID for both same-column and cross-column drops. Columns show drop feedback; movement is intentionally not persisted yet. Next checkpoint: make cards sortable drop targets so the UI can identify the destination card and derive neighbor positions.

User runtime/static evidence (2026-09-22): each card now uses `useSortable` inside its column's ordered `SortableContext`; columns remain droppable for empty-space and empty-column targets. User reports the interaction works, and frontend lint plus production build pass. Next checkpoint: derive nullable neighbor positions from the destination cards without issuing the mutation yet.

User runtime evidence (2026-09-23): drag end now derives nullable neighbor positions, calls the typed MoveCard client, and invalidates the exact `['board', boardId]` query after success. The refetched board renders the card in its persisted destination column. Next checkpoint: verify same-column top/middle/bottom ordering and failed-move feedback before considering optimistic cache updates.

Product direction captured (2026-09-21): evolve Boardsync toward a Jira-style dashboard with user authentication, board creation/navigation, ticket assignment, comments, and image attachments. Sequence these after a deployable core board release so authentication/ownership precedes user assignment and storage design precedes uploads.

Static implementation (2026-09-27): added a pre-auth board dashboard with typed create/open flow, board-level column creation, confirmed column deletion, exact-query invalidation, stable board ordering, and query-path indexes in migration 005. DeleteColumn now scopes its SQL by board and column and returns 404 when the pair does not exist. Backend build, frontend lint, and frontend production build pass. Runtime HTTP/database checks remain pending.

Static implementation (2026-09-28): replaced client-supplied ordering numbers with neighbor IDs for cards and columns. The API resolves and validates adjacent siblings, blocks cross-board card moves, calculates fractional positions centrally, and serializes position reads/writes with parent-row locks and transactions. Added frontend column drag reordering and eight xUnit ordering tests. Runtime HTTP/database and drag-interaction verification remain pending.
