# Production decisions

## Identity is a server concern

The frontend sends an access token. The API validates its signature, issuer,
audience, and expiry, then reads the user ID from the `sub` claim. Board and card
queries include that user ID in their SQL access predicate. A `boardId` alone
identifies a resource; it does not prove permission to use it.

Resources outside the current user's board access return 404. This avoids revealing
whether another user's guessed board or card ID exists.

Board ownership and membership are different roles. The owner adds an existing
registered user by email. Both can work on columns, cards, comments, reactions,
and attachment links. Only the owner can rename or delete the board or change
membership. Assignment accepts only the owner or a current member; removing a
member clears their card assignments in the same transaction. The list/detail
responses expose `isOwner` so the UI can hide owner controls, while SQL remains
the final authorization boundary.

## Access and refresh tokens have different jobs

The short-lived JWT makes normal API calls stateless. It stays in memory and is
lost when the page closes or reloads. The refresh token restores the session and
is stored in an HttpOnly cookie so application JavaScript cannot read it.

Refresh tokens are random opaque values. PostgreSQL stores only their hashes and
the API rotates them after use. Logout revokes the current token. This supports
server-side revocation without storing every access token.

Current limitation: reusing a rotated refresh token revokes all active sessions
for that user. That detects theft aggressively, but nearly simultaneous refreshes
from two tabs can also trigger it. A mature multi-device design would add token
families and a short concurrency grace window.

## The database enforces the durable model

Dapper keeps SQL visible and parameterized. Foreign keys and cascade rules
preserve relationships, while transactions and parent-row locks serialize
fractional ordering changes within a board or column. The server accepts neighbor
IDs and calculates positions itself, so clients cannot invent ordering values or
reference siblings from another board.

Repeated midpoint insertion eventually needs normalization. Before real-time
collaboration, add a threshold that rewrites one container's positions to spaced
integers within the same transaction.

## Deployment configuration stays outside features

Local development uses `.NET user-secrets` and an Npgsql key/value connection
string. Render supplies `DATABASE_URL` as a PostgreSQL URI. One startup resolver
normalizes those inputs before registering the connection factory. Handlers do
not know which platform hosts the application.

The API runs in a Linux container as the image's non-root `app` user. Render owns
TLS at its proxy; forwarded headers restore the original scheme before HTTPS and
cookie behavior run. CORS permits one configured frontend origin with credentials.
The API accepts the forwarded HTTPS scheme but does not accept forwarded client
IP addresses. Until Render's proxy addresses are explicitly trusted, auth rate
limits use the direct proxy connection address. This makes the limit shared by
visitors on the small demo and prevents a caller-supplied `X-Forwarded-For` value
from choosing its own rate-limit bucket.

In production, Next.js rewrites same-origin `/backend/*` requests to Render. This
matters because `vercel.app` and `onrender.com` are different sites; a browser may
block a refresh cookie set through a direct cross-site request. The proxy lets the
cookie remain HttpOnly, Secure, SameSite=Lax, and scoped to the proxied auth path.

## Collaboration increment and attachment boundary

Boards now support member assignment, comments, and one reaction per user/emoji
pair. Reaction toggles serialize on the card row so concurrent toggles cannot
create duplicate pairs. Comments retain their author and timestamp. Deleting a
card cascades to its activity and links.

Attachments are labeled HTTP(S) links. The API validates the scheme and does not
fetch remote content, which avoids storing files on Render's non-durable local
disk. Actual image/file uploads need object storage, size/type limits,
access-controlled downloads, and a deletion lifecycle. Notifications and
real-time presence remain later work.
