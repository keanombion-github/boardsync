"use client";

import {
  useMutation,
  useQuery,
  useQueryClient,
} from "@tanstack/react-query";
import Link from "next/link";
import {
  getBoardById,
  moveCard,
  reorderColumn,
  type BoardDetail,
} from "@/lib/api/boards";
import BoardColumn from "./board-column";
import { CreateColumnDialog } from "./create-column-dialog";
import {
  closestCenter,
  DndContext,
  type DragEndEvent,
} from "@dnd-kit/core";
import {
  arrayMove,
  horizontalListSortingStrategy,
  SortableContext,
} from "@dnd-kit/sortable";

type BoardViewProps = {
  boardId: string;
};

type DropTargetType = "card" | "column";

type MoveNeighbors = {
  beforeCardId: string | null;
  afterCardId: string | null;
};

type ColumnNeighbors = {
  beforeColumnId: string | null;
  afterColumnId: string | null;
};

function getMoveNeighbors(
  board: BoardDetail,
  cardId: string,
  destinationColumnId: string,
  overId: string,
  overType: DropTargetType,
): MoveNeighbors | null {
  const destinationColumn = board.columns.find(
    (column) => column.id === destinationColumnId,
  );

  if (!destinationColumn) {
    return null;
  }

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
    const lastCard = destinationCards.at(-1);

    return {
      beforeCardId: lastCard?.id ?? null,
      afterCardId: null,
    };
  }

  const targetIndex = destinationCards.findIndex(
    (card) => card.id === overId,
  );

  if (targetIndex === -1) {
    return null;
  }

  const targetCard = destinationCards[targetIndex];

  if (!targetCard) {
    return null;
  }

  const previousCard = destinationCards[targetIndex - 1];

  return {
    beforeCardId: previousCard?.id ?? null,
    afterCardId: targetCard.id,
  };
}

function getColumnNeighbors(
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

  const reorderedColumns = arrayMove(
    board.columns,
    activeIndex,
    overIndex,
  );
  const newIndex = reorderedColumns.findIndex(
    (column) => column.id === columnId,
  );

  return {
    beforeColumnId: reorderedColumns[newIndex - 1]?.id ?? null,
    afterColumnId: reorderedColumns[newIndex + 1]?.id ?? null,
  };
}

export function BoardView({ boardId }: BoardViewProps) {
  const {
    data: board,
    isLoading,
    isError,
  } = useQuery({
    queryKey: ["board", boardId],
    queryFn: () => getBoardById(boardId),
  });

  const queryClient = useQueryClient();

  const moveCardMutation = useMutation({
    mutationFn: moveCard,

    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["board", boardId],
      });
    },
  });

  const reorderColumnMutation = useMutation({
    mutationFn: reorderColumn,

    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ["board", boardId],
      });
    },
  });

  function handleDragEnd(event: DragEndEvent) {
    const { active, over } = event;

    if (
      !board
      || !over
      || moveCardMutation.isPending
      || reorderColumnMutation.isPending
    ) {
      return;
    }

    const activeId = String(active.id);
    const overId = String(over.id);
    const activeType = active.data.current?.type;
    const destinationColumnId = over.data.current?.columnId;
    const overType = over.data.current?.type;

    if (activeType === "column") {
      if (
        typeof destinationColumnId !== "string"
        || activeId === destinationColumnId
      ) {
        return;
      }

      const neighbors = getColumnNeighbors(
        board,
        activeId,
        destinationColumnId,
      );

      if (!neighbors) {
        return;
      }

      reorderColumnMutation.mutate({
        boardId,
        columnId: activeId,
        beforeColumnId: neighbors.beforeColumnId,
        afterColumnId: neighbors.afterColumnId,
      });
      return;
    }

    if (
      activeType !== "card" ||
      typeof destinationColumnId !== "string" ||
      (overType !== "card" && overType !== "column")
    ) {
      return;
    }

    if (activeId === overId) {
      return;
    }

    const neighbors = getMoveNeighbors(
      board,
      activeId,
      destinationColumnId,
      overId,
      overType,
    );

    if (!neighbors) {
      return;
    }

    moveCardMutation.mutate({
      id: activeId,
      columnId: destinationColumnId,
      beforeCardId: neighbors.beforeCardId,
      afterCardId: neighbors.afterCardId,
    });
  }

  if (isLoading) {
    return <p>Loading board...</p>;
  }

  if (isError) {
    return <p>Failed to load board.</p>;
  }

  if (!board) {
    return <p>Board data is unavailable.</p>;
  }

  return (
    <section className="p-6">
      <div className="mb-6 flex items-center justify-between gap-4">
        <div>
          <Link href="/" className="text-sm text-gray-500 hover:text-gray-900">
            Back to boards
          </Link>
          <h1 className="mt-1 text-2xl font-bold">{board.name}</h1>
        </div>
        <CreateColumnDialog boardId={boardId} />
      </div>

      {moveCardMutation.isError && (
        <p role="alert" className="mb-4 text-sm text-red-600">
          {moveCardMutation.error.message}
        </p>
      )}

      {reorderColumnMutation.isError && (
        <p role="alert" className="mb-4 text-sm text-red-600">
          {reorderColumnMutation.error.message}
        </p>
      )}

      <DndContext collisionDetection={closestCenter} onDragEnd={handleDragEnd}>
        {board.columns.length === 0 ? (
          <p className="rounded-lg border border-dashed p-8 text-center text-sm text-gray-600">
            This board has no columns yet. Add one to define its first workflow
            stage.
          </p>
        ) : (
          <SortableContext
            items={board.columns.map((column) => column.id)}
            strategy={horizontalListSortingStrategy}
          >
            <div className="flex items-start gap-4 overflow-x-auto pb-4">
              {board.columns.map((column) => (
                <BoardColumn
                  key={column.id}
                  column={column}
                  boardId={boardId}
                />
              ))}
            </div>
          </SortableContext>
        )}
      </DndContext>
    </section>
  );
}
