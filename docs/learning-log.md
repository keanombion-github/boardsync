# Learning Evidence

Do not infer mastery from AI-generated code or completed files.

| Topic | Evidence | Next ownership check |
|---|---|---|
| React/Next.js/TypeScript | User reports strong proficiency | Explain feature-specific API/cache decisions only |
| VSA, DI, Dapper, CQRS | Slices exist; independent explanation not assessed | Trace binding, validation, handler, SQL, response |
| DTO mapping | Previous guide records a mapping/return-type lesson | Explain selected columns versus DTO shape |
| Fractional ordering | Implementations exist; edge cases remain | Predict empty/top/middle/bottom and concurrent inserts |

After a checkpoint, add one dated row with the user's demonstrated explanation/implementation and remaining gap. Change Skills.md levels only when evidence supports it.
| Endpoint names (2026-09-13) | User independently renamed MoveCard and explained identity/debugging value | Clarify that MapPut defines URL; WithName supplies unique metadata for named link generation |
| Empty destination (2026-09-14) | User explained that hasOtherCards false permits moving the card into its destination; reviewed implementation checks only when both neighbor positions are absent | Verify exclusion of the moving card and rejection when another card occupies the destination; concurrency remains a later topic |
| Nested board DTO (2026-09-16) | User explained stable cards arrays for frontend use and grouping by unique column ID instead of a duplicateable name; implemented flat-row grouping and card mapping | Trace LEFT JOIN null rows through grouping and explain why card filtering happens inside each column group |
| Query cache identity (2026-09-20) | User explained that multiple boards require the correct board ID when invalidating the cache | Distinguish targeted `["board", boardId]` invalidation from the broader `["board"]` prefix invalidation |
| Destructive mutation lifecycle (2026-09-21) | User implemented and runtime-tested a delete alert with an explicit mutation trigger, pending state, error state, exact-board refetch, and success-only close | Explain why a 204 response must not be parsed as JSON and why immediate dialog close hides failure feedback |
| MoveCard HTTP boundary (2026-09-21) | User independently implemented the typed frontend request using the card ID in the route and destination/neighbors in the body; lint and build pass | Derive correct neighbor positions from a dnd-kit drop for empty, top, middle, and bottom cases |
| Nested resource scoping (2026-09-27) | User explained that including `board_id` with the unique column ID adds an extra ownership/safety check | Connect the board/column predicate to future authorization and explain why it returns 404 for a mismatched pair |
