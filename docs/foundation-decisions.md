# Boardsync Foundation Decisions

This note records the reasons behind the current pre-authentication foundation. It describes implemented behavior, known tradeoffs, and the decisions that should be revisited as Boardsync grows.

## Server data remains authoritative

TanStack Query stores fetched boards in a client cache. Create, update, move, and delete operations write through the API first. On success, the frontend invalidates the exact affected query key and refetches the server representation.

- Board list key: `["boards", ownerId]`
- Board detail key: `["board", boardId]`

This prevents a failed write from being presented as persisted data. It costs an additional GET after each mutation and can produce a brief visual snap-back during drag operations. Optimistic updates are deferred until the write paths and rollback behavior are tested.

## URLs identify resource scope

Column routes are nested under a board:

```text
POST   /api/boards/{boardId}/columns
DELETE /api/boards/{boardId}/columns/{columnId}
```

The route identifies the parent resource; the request body describes the new or changed resource. The delete handler filters by both IDs. This prevents a route for one board from deleting a column belonging to another board and prepares the query for future authorization checks.

## Destructive cascades require explicit UI confirmation

PostgreSQL defines `cards.column_id` with `ON DELETE CASCADE`. Deleting a column therefore deletes all cards inside it in the same database operation. The UI uses an alert dialog, displays the affected card count, disables controls while the request is pending, and stays open when an error occurs.

The database cascade is preferred over issuing one DELETE per card because it is atomic and avoids partial cleanup. Recovery or archival would require a later soft-delete design.

## Mutation state belongs near the interaction

Create forms and delete dialogs own their own mutation, pending, and error state. The board view owns card movement because drag events and neighbor calculation happen at that level. This keeps feature behavior close to the component that initiates it without introducing a global client-state store for server data.

## Demo identity is an explicit temporary boundary

Before JWT authentication, the dashboard reads `NEXT_PUBLIC_DEMO_OWNER_ID`. It is a development identity, not authorization. The value is public in the browser bundle and the API currently trusts caller-supplied owner IDs.

Authentication must replace this design by deriving the user ID from verified token claims on the server. The client should then stop sending or configuring owner identity.

## Indexes follow observed access paths

Migration `005_add_query_indexes.sql` adds composite indexes for:

- Boards filtered by owner and ordered by creation time.
- Columns filtered by board and ordered by fractional position.
- Cards filtered by column and ordered by fractional position.

Indexes speed reads by adding storage and write-maintenance cost. These three match current high-frequency queries and avoid indexing speculative future fields.

## Fractional ordering is useful but incomplete

Cards and columns use positions between their neighbors, avoiding updates to every item during most moves. The browser sends nullable neighbor IDs rather than raw floating-point positions. The backend loads the ordered siblings, verifies that the supplied IDs are real and adjacent, and calculates the new position itself. This keeps database ordering rules behind the API trust boundary.

Position-changing operations lock their containing board or column and use a transaction before reading sibling positions. Requests for the same container therefore calculate positions sequentially rather than both reading the same previous maximum.

Repeated midpoint inserts can still eventually produce values too close together. Equal existing neighbor positions are rejected because no valid midpoint exists. The project still needs a later normalization strategy that rewrites a container to simple spaced positions inside a transaction.

Card and column drag behavior share the same server-side fractional-position calculator, while their frontend collision handling remains separate. This keeps one ordering invariant without mixing two UI interactions.
