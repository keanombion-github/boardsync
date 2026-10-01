import { arrayMove } from "@dnd-kit/sortable";
import type { BoardDetail } from "@/lib/api/boards";

export type DropTargetType = "card" | "column";

export type MoveNeighbors = {
  beforeCardId: string | null;
  afterCardId: string | null;
};

export type ColumnNeighbors = {
  beforeColumnId: string | null;
  afterColumnId: string | null;
};

export function getMoveNeighbors(
  board: BoardDetail,
  cardId: string,
  destinationColumnId: string,
  overId: string,
  overType: DropTargetType,
): MoveNeighbors | null {
  const destinationColumn = board.columns.find(
    (column) => column.id === destinationColumnId,
  );

  if (!destinationColumn) return null;

  const destinationCards = destinationColumn.cards.filter(
    (card) => card.id !== cardId,
  );
  const sourceColumn = board.columns.find((column) =>
    column.cards.some((card) => card.id === cardId),
  );

  if (overType === "card" && sourceColumn?.id === destinationColumnId) {
    const activeIndex = destinationColumn.cards.findIndex(
      (card) => card.id === cardId,
    );
    const overIndex = destinationColumn.cards.findIndex(
      (card) => card.id === overId,
    );

    if (activeIndex === -1 || overIndex === -1 || activeIndex === overIndex) {
      return null;
    }

    const reorderedCards = arrayMove(
      destinationColumn.cards,
      activeIndex,
      overIndex,
    );
    const newIndex = reorderedCards.findIndex((card) => card.id === cardId);

    return {
      beforeCardId: reorderedCards[newIndex - 1]?.id ?? null,
      afterCardId: reorderedCards[newIndex + 1]?.id ?? null,
    };
  }

  if (overType === "column") {
    return {
      beforeCardId: destinationCards.at(-1)?.id ?? null,
      afterCardId: null,
    };
  }

  const targetIndex = destinationCards.findIndex((card) => card.id === overId);
  if (targetIndex === -1) return null;

  return {
    beforeCardId: destinationCards[targetIndex - 1]?.id ?? null,
    afterCardId: destinationCards[targetIndex]?.id ?? null,
  };
}

export function getColumnNeighbors(
  board: BoardDetail,
  columnId: string,
  overColumnId: string,
): ColumnNeighbors | null {
  const activeIndex = board.columns.findIndex(
    (column) => column.id === columnId,
  );
  const overIndex = board.columns.findIndex(
    (column) => column.id === overColumnId,
  );

  if (activeIndex === -1 || overIndex === -1 || activeIndex === overIndex) {
    return null;
  }

  const reorderedColumns = arrayMove(board.columns, activeIndex, overIndex);
  const newIndex = reorderedColumns.findIndex(
    (column) => column.id === columnId,
  );

  return {
    beforeColumnId: reorderedColumns[newIndex - 1]?.id ?? null,
    afterColumnId: reorderedColumns[newIndex + 1]?.id ?? null,
  };
}
