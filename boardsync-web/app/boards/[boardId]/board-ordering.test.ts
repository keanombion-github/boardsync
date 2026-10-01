import { describe, expect, it } from "vitest";
import type { BoardDetail } from "@/lib/api/boards";
import { getColumnNeighbors, getMoveNeighbors } from "./board-ordering";

const board: BoardDetail = {
  id: "board",
  name: "Test board",
  isOwner: true,
  columns: [
    {
      id: "todo",
      name: "To do",
      position: 1,
      cards: [
        { id: "a", title: "A", description: null, position: 1, assigneeId: null, assigneeName: null },
        { id: "b", title: "B", description: null, position: 2, assigneeId: null, assigneeName: null },
        { id: "c", title: "C", description: null, position: 3, assigneeId: null, assigneeName: null },
      ],
    },
    {
      id: "doing",
      name: "Doing",
      position: 2,
      cards: [
        { id: "d", title: "D", description: null, position: 1, assigneeId: null, assigneeName: null },
        { id: "e", title: "E", description: null, position: 2, assigneeId: null, assigneeName: null },
      ],
    },
    { id: "done", name: "Done", position: 3, cards: [] },
  ],
};

describe("getMoveNeighbors", () => {
  it("places a cross-column card before the target card", () => {
    expect(getMoveNeighbors(board, "a", "doing", "e", "card")).toEqual({
      beforeCardId: "d",
      afterCardId: "e",
    });
  });

  it("appends a card when the column body is the drop target", () => {
    expect(getMoveNeighbors(board, "a", "doing", "doing", "column")).toEqual({
      beforeCardId: "e",
      afterCardId: null,
    });
  });

  it("returns empty neighbors for an empty destination", () => {
    expect(getMoveNeighbors(board, "a", "done", "done", "column")).toEqual({
      beforeCardId: null,
      afterCardId: null,
    });
  });

  it("derives neighbors after a same-column reorder", () => {
    expect(getMoveNeighbors(board, "a", "todo", "c", "card")).toEqual({
      beforeCardId: "c",
      afterCardId: null,
    });
  });
});

describe("getColumnNeighbors", () => {
  it("derives neighbors after moving a column", () => {
    expect(getColumnNeighbors(board, "todo", "done")).toEqual({
      beforeColumnId: "done",
      afterColumnId: null,
    });
  });

  it("does nothing when a column is dropped on itself", () => {
    expect(getColumnNeighbors(board, "todo", "todo")).toBeNull();
  });
});
