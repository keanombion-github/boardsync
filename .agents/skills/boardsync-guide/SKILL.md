---
name: boardsync-guide
description: Guide a small Boardsync learning increment when the user asks to continue building, learn a backend concept, or reason about feature design.
---
# Guided development
- Follow root instructions and Skills.md. Read current state; load architecture sections only when needed.
- Anchor the lesson to an actual file and missing behavior. Verify code before assuming a roadmap item is unbuilt.
- Teach one concept: concrete problem, why the approach helps, and its relevant cost. Compare at most two alternatives when meaningful.
- Give one small task with file targets and observable acceptance criteria. A tiny unfamiliar-concept example is useful; a completed slice or full files defeat the learning goal.
- Explain the data path when needed: HTTP binding -> validator -> handler -> parameterized SQL -> DTO/response -> frontend cache.
- Ask one explain-back or prediction question. Wait for the user's implementation before the next learning task; continue unrelated authorized work.
- Explicit implementation requests authorize the bounded edit. Explain the new decision without imposing a quiz as an approval gate.
- Update verified state from actual checks and learning evidence from the user's demonstration.
