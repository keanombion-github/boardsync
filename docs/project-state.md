# Verified Project State

Reviewed 2026-10-01. Core Kanban, authentication, owner authorization, production
middleware, CI, and deployment configuration are implemented. External deployment
has not been performed.

Collaboration increment, 2026-10-01: migrations 007-009 add comments, reactions,
attachment links, and board membership. The API supports member-scoped board
work, assignee changes, comment creation, reaction toggles, and link add/remove.
The frontend uses Inter, a shared left board sidebar, a members dialog, an
assignee badge/selector, and a ticket activity dialog. The latest .NET Release
build/test (14 passed), frontend lint/build, and Vitest (6 passed) all pass.
Live two-account HTTP checks confirmed access before/after invite and removal,
member-created card, assignment readback, comment/reaction, attachment
add/remove, and URL rejection. Disposable accounts were deleted. New browser
interactions and hosted behavior still need manual acceptance.

Drag-preview and ticket-discovery fix, 2026-10-02: the board renders a floating
card overlay during drag and a dashed insertion slot in a hovered destination
column. Cancel clears the preview; drop still uses the existing server-calculated
neighbor request. Every card shows its assignee or "Unassigned" and has a
labeled **Details & comments** action. Frontend lint, production build, and six
ordering tests pass. Browser automation could not start on this workstation,
so visual hover and modal interaction remain for the user's manual test.

Local runtime fix, 2026-10-02: a Debug API process on port 5230 and Next dev
process on port 3000 had started before the collaboration code was added. The
old API returned 404 for the new activity/members routes. Both local services
were restarted from current source; `/health` and the frontend return 200, and
the activity route now returns 401 without a token, proving it is mapped and
protected. A disposable authenticated smoke on port 5230 returned 200 for
members and assignment, 201 for comment creation, and 200 for activity readback
with the assignee and comment; its board/account were removed. The ticket dialog
now explains member-loading failures and places comments immediately after the
assignee selector. Browser interaction still needs the user's confirmation.

## Implemented source
- Backend and tests target net10.0. CI and the Docker runtime use .NET 10, and
  `global.json` pins the 10.0.4xx SDK feature band.
- Program.cs maps CreateBoard, GetBoards, GetBoardById; CreateColumn, DeleteColumn, ReorderColumn; CreateCard, UpdateCard, DeleteCard, MoveCard.
- DbUp embeds migrations 001-009 for users, boards, columns, cards, indexes,
  refresh tokens, card activity, attachment links, and board membership; API
  startup runs migrations.
- GetBoardById returns a board with ordered columns and nested ordered card arrays using one joined query.
- Frontend: Next.js 16.3.8, React 19.2.4, TanStack Query, authenticated dashboard/detail
  routes, shared board sidebar, card CRUD/activity/assignment, persisted movement,
  column create/delete, and column drag reordering.
- JWT access tokens, rotating hashed refresh tokens, HttpOnly cookies, register/login/
  refresh/logout/me endpoints, and claim-derived ownership are implemented.
- A GitHub Actions workflow is configured to build/test the backend and lint/build
  the frontend; it has not run on the remote repository yet. A Render
  Blueprint, non-root API Dockerfile, health check, and Vercel runbook are present.
- Fourteen xUnit tests cover fractional positioning, token generation, and Render
  database-URI conversion. Six Vitest tests cover card and column neighbor
  derivation for drag ordering.

## Known issues / learning tasks
1. The public Git history contains an old local PostgreSQL password. It was rotated
   and the historic credential was rejected on 2026-10-01. The role is shared by a
   cluster with four non-template databases; other local clients using `postgres`
   may need their saved passwords updated.
2. Repeated midpoint insertion can exhaust floating-point gaps. Define a position-normalization threshold and transaction before real-time collaboration.
3. Refresh-token reuse detection is intentionally strict and can sign out concurrent
   tabs that refresh at nearly the same moment.
4. Docker cannot be executed on this workstation because Docker is not installed;
   the Dockerfile needs its first build in CI or Render.
5. Render free PostgreSQL expires after 30 days; use paid or alternative persistent
   storage for a durable public portfolio demo.
6. The browser automation runtime failed to initialize, and no Render/Vercel CLI
   credentials are available here. Hosted deployment and browser acceptance still
   need account access and a live smoke test.

## Verification
- Backend .NET 10 Release build and publish passed with zero warnings/errors. The
  local .NET 10 API started, checked DbUp migrations, and `/health` returned 200.
- All fourteen xUnit tests pass.
- Frontend ESLint and the Next.js 16.3.8 production build pass.
- All six Vitest ordering tests pass.
- NuGet reported no known vulnerable packages. npm production audit reports zero
  vulnerabilities after the Next.js security upgrade.
- Runtime evidence is recorded below. The 2026-09-27 additions have static build verification only; migration 005 and the new mutations have not been exercised against the live database.

## Resume checkpoint
Walk through the new membership, assignment, activity, and sidebar interactions
in `docs/feature-review.md`, then validate the Docker image in Render and
perform the deployment runbook with real Render and Vercel URLs.

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

User runtime evidence (2026-09-28): user reports the completed dashboard, board/column/card CRUD, card movement, and column ordering flows are working. Phase 2a is treated as complete. Phase 2b begins with registration and secure password persistence.

Implementation/runtime evidence (2026-10-01): register/login/refresh/logout/me and
claim-derived owner scoping are implemented. Disposable-account checks confirmed
registration 201, duplicate conflict 409, wrong-password 401, authenticated me
200, cross-owner board read 404, refresh rotation 200, logout 204, and refresh
after logout 401. Board rename returned 200 and read back the new name; board
delete returned 204 and subsequent read returned 404.

Deployment-path evidence (2026-10-01): a Next.js same-origin rewrite proxied login
to the API, preserved the HttpOnly refresh cookie at `/backend/api/auth`, and
successfully refreshed the session through the proxy. Backend build and 12 tests,
frontend lint/build and 6 tests, NuGet vulnerability audit, and npm production
audit pass. Docker execution remains unavailable locally.

Visual review (2026-10-01): the login, dashboard, empty board, and populated-column
states were inspected in the browser. Board columns, cards, forms, and confirmation
dialogs now use the same dark glass visual system as authentication and dashboard
screens. A local account, board, and column were created through the rendered UI;
the browser-to-API requests returned 200/201 and persisted across refresh.

.NET 10 migration evidence (2026-10-01): installed SDK 10.0.401 in the user profile,
retargeted the API and test project to net10.0, aligned JWT/Serilog packages, Docker,
and CI, and added `global.json`. Release build and publish pass with zero warnings;
all 12 backend tests pass on net10.0 and the NuGet vulnerability audit is clean.

Release API smoke (2026-10-01): rotated the shared local PostgreSQL password,
confirmed the previously committed value is rejected, and verified `/health` 200
using the new .NET user-secret. After removing two invalid `Location` headers and
rejecting unrepresentable fractional positions, the .NET 10 Release build passes
with zero warnings and all 14 xUnit tests pass. A disposable-account HTTP smoke
passed 29 checks across registration/login/me/refresh/logout, board CRUD, column
create/reorder/delete, card create/edit/move/delete, ordered readback, validation,
owner isolation, and response headers. Smoke data was removed afterward.
Eleven invalid login requests with distinct `X-Forwarded-For` values reached the
same rate-limit bucket; the eleventh returned 429 after the API stopped accepting
forwarded client IP addresses.

Frontend release review (2026-10-01): logout now keeps the local session intact
when its API request fails and shows a retryable error; drag errors use readable
contrast on the dark board. Frontend ESLint and production build pass after the
change. A browser network-failure interaction still needs manual confirmation.

Ticket modal UX (2026-10-02): the card title and pencil now open one ticket modal
containing title/description editing, assignment, comments, attachment links,
reactions, and a confirmed delete action. The card retains a separate drag grip.
Frontend production build and six tests pass; browser interaction remains to be
checked manually.

No-cost deployment preparation (2026-10-02): the runbook now describes Netlify
for Next.js, Render Free for the API, and Neon Free for PostgreSQL. PostgreSQL
URI conversion now retains `sslmode` and `channel_binding` options needed by
hosted connection strings; Release build and all 15 backend tests pass. Public
signup is still open and resource quotas/list-size bounds have not been
implemented.

Hosted API checkpoint (2026-10-02): the user deployed commit `7205ed8` to the
Render `boardsync-api` Free web service. Their deployment screenshot shows DbUp
applying migrations through 009 and a successful live status. An independent
request to `https://boardsync-api.onrender.com/health` returned HTTP 200 with
`Healthy`. Hosted authentication remains untested.

Netlify first deploy (2026-10-03): the dashboard shows `boardsync-web` published
from commit `7205ed8`, but independent requests to its root, `/backend/health`,
and `/backend/api/auth/me` all returned Netlify's HTML 404 page. The next
deploy's public log shows `next build` generated `/`, `/login`, `/register`, and
`/boards/[boardId]`, but Netlify uploaded raw `.next` files with zero functions.
The Next.js adapter was not run. A file-based adapter dependency and
`netlify.toml` are prepared locally; production behavior still needs a new
deploy and HTTP verification. Confirm `API_PROXY_TARGET` if the frontend loads
but `/backend/health` remains unavailable.
