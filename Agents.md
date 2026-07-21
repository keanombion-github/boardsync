# Project Agent Instructions

## Role

You are helping build a production-quality full-stack portfolio project called **Boardsync**.

Read `Skills.md` for the user's skill level.
Read `boardsync-build-guide.md` for project context and architecture.

### Mandatory Every Step

- **Always re-read** `Agents.md`, `Skills.md`, and `boardsync-build-guide.md` at the start of every prompt/step.
- **Always auto-run commands** (build, lint, test, etc.) — never ask for permission. Just run them.

---

## Tech Stack

Frontend: React, Next.js App Router, TypeScript, TailwindCSS, shadcn/ui, TanStack Query

Backend: .NET 8 Web API, Dapper, PostgreSQL, Vertical Slice Architecture, CQRS, DI

---

## Coding Standards

DO:

- Write small, reusable components
- Use strong typing everywhere
- Prefer composition over inheritance
- Use async/await
- Use functional React components
- Use descriptive variable and function names

DON'T:

- Use `any` in TypeScript
- Duplicate logic across files
- Create unnecessary abstractions
- Write deeply nested code (max 2–3 levels)
- Add unrelated refactors to a task

---

## Workflow

Before writing code:

1. Search existing code for similar patterns.
2. Reuse existing components and utilities.
3. Briefly explain the approach.
4. Then implement.

While editing:

- Only change files required for the task.
- Preserve existing code style and comments.
- Do not rename or restructure unrelated code.

After implementation:

- Run `dotnet build` (backend) or `npm run build` (frontend).
- Fix all TypeScript / C# compiler errors.
- Check lint.
- List any remaining issues honestly.

---

## Response Efficiency (IMPORTANT)

These rules reduce token usage. Follow them strictly:

- Show only changed code. Do not repeat unchanged code.
- Use the smallest code block that shows the change with enough context to locate it.
- If a fix is one line, describe it in plain text instead of a code block.
- Do not add filler phrases like "Great question!" or "Sure, I'd be happy to help!"
- Use bullet points, not paragraphs.
- Do not re-explain concepts the user already knows (check Skills.md).
- Do not summarize what you just did at the end unless asked.
- When multiple files change, show each file separately with its path.
- Prefer diffs or partial snippets over full file rewrites.