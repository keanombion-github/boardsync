# Boardsync Build Guide

This document is the single source of truth for the Boardsync project. Paste it at the start of any new AI conversation about this project.

Other files to reference:
- `Agents.md` — coding standards and response rules
- `Skills.md` — user skill levels and teaching preferences

---

## 1. Current Status

**Phase:** 1 — Foundation
**Current Step:** GetBoards slice implementation
**Last completed:** Initialized Git repo, tested CreateBoard slice, created dummy user in PostgreSQL
**Blockers:** None, currently fixing compilation errors in GetBoards slice

> Update this section at the end of each session so the next session starts fast.

---

## 2. Project Overview

**Boardsync** is a collaborative Kanban board (simplified Trello) built as a portfolio project to demonstrate hireable full-stack skills.

**Key goal:** Every architectural and technical decision must be explainable in an interview.

---

## 3. Tech Stack

| Layer | Choice | Why |
|---|---|---|
| Backend | ASP.NET Core Web API (.NET 8) | Industry standard, strong job demand |
| Data access | Dapper | Shows raw SQL skill, not just ORM magic |
| Database | PostgreSQL 18 (local install) | Widely used, free-tier friendly, no Docker needed |
| Architecture | Vertical Slice Architecture | Modern, scales well, easy to explain |
| Validation | FluentValidation | Clean, testable, separate from business logic |
| Auth | JWT (access + refresh tokens) | Standard for SPA ↔ API auth |
| Logging | Serilog (structured) | Real observability practice |
| Frontend | Next.js (App Router) + TypeScript | In-demand, SSR-capable |
| Server state | TanStack Query | Caching, revalidation, loading states |
| Client state | Zustand | Lightweight, simple API for UI-only state |
| Styling | TailwindCSS + shadcn/ui | Fast, modern, consistent design system |
| Drag & drop | dnd-kit | Accessible, performant, React-native |
| Real-time | SignalR + Redis backplane | Distributed real-time experience |
| Testing | xUnit (backend), Vitest (frontend) | Non-negotiable for portfolio quality |
| CI/CD | GitHub Actions | Automated build → test → deploy |
| Backend hosting | Render (API + Postgres) | Free tier, real deployment |
| Frontend hosting | Vercel | Optimized for Next.js |
| DB migrations | DbUp | Simple SQL scripts, version-controlled |

---

## 4. Architecture

### Vertical Slice Architecture (Backend)

Each feature is a self-contained "slice." Everything a feature needs — endpoint, command/query, handler, validator, SQL — lives in one folder.

```
Boardsync.Api/
├── Program.cs
├── appsettings.json
├── Common/
│   ├── Database/             # Dapper connection factory
│   ├── Middleware/            # Global error handling, auth
│   ├── Extensions/            # Service registration helpers
│   ├── Behaviors/             # Pipeline behaviors (validation, logging)
│   └── Models/                # Shared response models (ApiResponse, PagedResult)
├── Features/
│   ├── Boards/
│   │   ├── CreateBoard/
│   │   │   ├── CreateBoardEndpoint.cs
│   │   │   ├── CreateBoardCommand.cs
│   │   │   ├── CreateBoardValidator.cs
│   │   │   └── CreateBoardHandler.cs
│   │   ├── GetBoards/
│   │   ├── GetBoardById/
│   │   └── DeleteBoard/
│   ├── Columns/
│   │   ├── CreateColumn/
│   │   └── ReorderColumns/
│   ├── Cards/
│   │   ├── CreateCard/
│   │   ├── MoveCard/
│   │   └── UpdateCard/
│   └── Auth/
│       ├── Register/
│       ├── Login/
│       └── RefreshToken/
├── Migrations/                # DbUp SQL migration scripts (numbered)
│   ├── 001_create_users_table.sql
│   ├── 002_create_boards_table.sql
│   └── ...
└── Boardsync.Api.Tests/
    └── Features/              # Mirrors Features/ structure
```

### Frontend Structure

```
boardsync-web/
├── app/
│   ├── (auth)/
│   │   ├── login/page.tsx
│   │   └── register/page.tsx
│   ├── boards/
│   │   ├── [boardId]/page.tsx
│   │   └── page.tsx
│   ├── layout.tsx
│   └── globals.css
├── components/
│   ├── ui/                    # shadcn/ui + custom generic components
│   └── board/                 # Board, Column, Card components
├── lib/
│   ├── api/                   # API client functions (one file per feature)
│   ├── hooks/                 # TanStack Query hooks (one file per feature)
│   ├── stores/                # Zustand stores
│   └── utils/                 # Shared helpers
└── types/                     # Shared TypeScript types/interfaces
```

---

## 5. Conventions

### API Response Format

All API responses use a consistent wrapper:

```json
{
  "success": true,
  "data": { },
  "error": null
}
```

Error responses:

```json
{
  "success": false,
  "data": null,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Board name is required.",
    "details": []
  }
}
```

### HTTP Status Codes

- `200` — Success (GET, PUT)
- `201` — Created (POST that creates a resource)
- `204` — No Content (DELETE)
- `400` — Validation error
- `401` — Not authenticated
- `403` — Not authorized
- `404` — Resource not found
- `500` — Unexpected server error (never expose stack traces)

### Naming Conventions

| Context | Convention | Example |
|---|---|---|
| C# classes | PascalCase | `CreateBoardHandler` |
| C# methods | PascalCase | `HandleAsync()` |
| C# variables | camelCase | `boardName` |
| SQL tables | snake_case, plural | `boards`, `board_members` |
| SQL columns | snake_case | `created_at`, `board_id` |
| TS components | PascalCase | `BoardCard.tsx` |
| TS functions | camelCase | `fetchBoards()` |
| TS types/interfaces | PascalCase | `Board`, `CreateBoardRequest` |
| API routes | kebab-case, plural | `/api/boards`, `/api/boards/{id}/columns` |
| Migration files | numbered + snake_case | `001_create_users_table.sql` |

### Git Workflow

- `main` — production-ready, deploy from here
- `dev` — integration branch
- Feature branches: `feature/create-board`, `feature/jwt-auth`
- Commit messages: conventional commits (`feat:`, `fix:`, `refactor:`, `docs:`, `test:`)
- Squash merge feature branches into `dev`

### Environment Variables

Backend (`appsettings.json` structure, overridden by env vars in production):

- `ConnectionStrings__DefaultConnection` — PostgreSQL connection string
- `Jwt__SecretKey` — JWT signing key (min 256 bits)
- `Jwt__Issuer` — Token issuer
- `Jwt__Audience` — Token audience
- `Jwt__AccessTokenExpiryMinutes` — e.g., 15
- `Jwt__RefreshTokenExpiryDays` — e.g., 7

Frontend (`.env.local`):

- `NEXT_PUBLIC_API_URL` — Backend API base URL

> NEVER commit secrets. Use `.env` files locally and platform env vars in production.

---

## 6. Database Schema (Core Tables)

```sql
-- Users
users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    display_name VARCHAR(100) NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
)

-- Boards
boards (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(200) NOT NULL,
    owner_id UUID NOT NULL REFERENCES users(id),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
)

-- Columns
columns (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    board_id UUID NOT NULL REFERENCES boards(id) ON DELETE CASCADE,
    name VARCHAR(200) NOT NULL,
    position DOUBLE PRECISION NOT NULL,  -- fractional ordering
    created_at TIMESTAMPTZ DEFAULT NOW()
)

-- Cards
cards (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    column_id UUID NOT NULL REFERENCES columns(id) ON DELETE CASCADE,
    title VARCHAR(300) NOT NULL,
    description TEXT,
    position DOUBLE PRECISION NOT NULL,  -- fractional ordering
    assigned_to UUID REFERENCES users(id),
    created_at TIMESTAMPTZ DEFAULT NOW(),
    updated_at TIMESTAMPTZ DEFAULT NOW()
)

-- Refresh tokens
refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token TEXT NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    revoked_at TIMESTAMPTZ
)
```

**Fractional positioning:** Items are ordered by a `DOUBLE PRECISION` position value. To insert between items at position 1.0 and 2.0, set position to 1.5. Rebalance positions periodically when values get too close. This avoids rewriting every row on reorder — great interview talking point.

---

## 7. Build Roadmap

Build in complete, working increments. Never leave features half-finished.

### Phase 1 — Foundation

- [ ] Scaffold .NET Web API with VSA folder structure
- [ ] Create `boardsync` database in local PostgreSQL
- [ ] Create Dapper connection factory
- [ ] Set up DbUp for migrations, run first migration (users + boards tables)
- [ ] Build one slice end-to-end: `CreateBoard` (endpoint → validator → handler → SQL)
- [ ] Build `GetBoards` slice
- [ ] Scaffold Next.js project with TailwindCSS + shadcn/ui
- [ ] Create API client, fetch and display boards from backend
- [ ] **Checkpoint:** browser → API → database → browser works

### Phase 2a — Core CRUD

- [ ] Add Columns slices: CreateColumn, ReorderColumns, DeleteColumn
- [ ] Add Cards slices: CreateCard, UpdateCard, MoveCard, DeleteCard
- [ ] Implement fractional positioning for ordering
- [ ] Build Board detail page (columns + cards layout)
- [ ] Add drag-and-drop with dnd-kit
- [ ] **Checkpoint:** can create a board, add columns, add cards, drag to reorder

### Phase 2b — Authentication

- [ ] Add JWT auth: Register, Login, RefreshToken slices
- [ ] Create auth middleware
- [ ] Add `[Authorize]` to protected endpoints
- [ ] Build login/register pages on frontend
- [ ] Add auth context/provider, protected routes
- [ ] Token refresh logic (intercept 401, retry with refresh token)
- [ ] **Checkpoint:** full auth flow works, boards are per-user

### Phase 3 — Production Hygiene

- [ ] FluentValidation on all slices
- [ ] Global error handling middleware (consistent error responses)
- [ ] Serilog structured logging (request/response, errors, key events)
- [ ] Rate limiting on auth endpoints
- [ ] CORS configuration
- [ ] Input sanitization review
- [ ] xUnit tests for handlers
- [ ] Vitest tests for key frontend components
- [ ] **Checkpoint:** error handling, logging, validation, and tests all in place

### Phase 4 — CI/CD + Deployment

- [ ] GitHub Actions: lint → test → build → deploy
- [ ] Deploy API + Postgres to Render
- [ ] Deploy Next.js to Vercel
- [ ] Environment variable management across platforms
- [ ] **Checkpoint:** push to main → auto deploys, live app accessible
- [ ] (Optional) Dockerize the API later if needed for deployment

### Phase 5 — Real-Time

- [ ] Add SignalR hub for board updates (card moved, card created, etc.)
- [ ] Redis backplane for scaling SignalR
- [ ] Presence indicators (who's viewing the board)
- [ ] Optimistic updates on frontend + reconcile with server broadcasts
- [ ] **Checkpoint:** two browser tabs see real-time changes

### Phase 6 — Polish for Portfolio

- [ ] Architecture diagram (Mermaid or Excalidraw)
- [ ] README with project overview, screenshots, tech decisions
- [ ] Write-up on 1–2 hard problems solved
- [ ] Record demo video/gif
- [ ] Final security + performance review
- [ ] **Checkpoint:** repo is interview-ready

---

## 8. Learning Goals

The primary purpose of this project is **skill development**, not just shipping a product.

**Priority focus areas:**
- Understanding Vertical Slice Architecture deeply — be able to explain every folder and file
- CQRS pattern — why separate reads from writes, when it matters
- Building confidence in .NET backend development (DI, middleware, configuration)
- Writing raw SQL with Dapper instead of hiding behind an ORM
- Full-stack data flow: browser → API → database → browser
- Making every technical decision interview-explainable

**How we work:**
- One concept at a time, fully understood before moving on
- Always explain *why* before *how* for new backend concepts
- Code is written to learn, not just to ship

---

## 9. Environment Setup

| Tool | Version | Notes |
|---|---|---|
| .NET SDK | 9.0.301 | Using .NET 9 (latest, compatible with .NET 8 patterns) |
| Node.js | v20.13.1 | For Next.js frontend |
| PostgreSQL | 18.0 | Installed locally as Windows service, no Docker |
| Docker | Skipped | Machine cannot support it; using local installs instead |

---

## 10. How to Start a Session

Paste this document (and optionally `Agents.md` + `Skills.md`) at the start of a new chat. Then say:

> "I'm on Phase [X], Step [Y]. Here's where I left off: [brief context]. Let's continue."

If first session: **"Let's start Phase 1."**

After each session, update the **Current Status** section at the top of this file.

---

## 11. Dev Log & Lessons Learned

### Phase 1 Foundation & The Copy-Paste Trap
**Progress:**
- Successfully built and tested the `CreateBoard` vertical slice (Endpoint -> Validator -> Handler -> SQL).
- Hit our first 500 error from a foreign key constraint and JSON parsing issue (passed a bad string instead of a real UUID), resolving it by looking at middleware logs!
- Started the `GetBoards` query slice.

**Lessons Learned:**
- **The Copy-Paste Trap in VSA:** When copying a Command (Write) to make a Query (Read), always remember to change the return type. Queries return *data* (like `Task<IEnumerable<BoardDto>>`), whereas Commands typically return just an ID.
- **Dapper Mapping:** Dapper's `QueryAsync<T>` expects `T` to match the columns you select. You can't select three columns (`id, name, owner_id`) and map them to a single `Guid`. You need a Data Transfer Object (DTO) or a C# Record to hold them.
- **Minimal API Parameter Binding:** You don't need to pass both `Guid ownerId` and `GetBoardsQuery query` into the endpoint. Using `[AsParameters] GetBoardsQuery query` tells Minimal API to map the URL query string directly to your object!
- **HTTP Status Codes:** `POST` = 201 Created. `GET` = 200 OK.
