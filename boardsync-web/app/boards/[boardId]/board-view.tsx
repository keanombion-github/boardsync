"use client";

import { useState } from "react";
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
  type Card,
} from "@/lib/api/boards";
import BoardColumn from "./board-column";
import { CreateColumnDialog } from "./create-column-dialog";
import { EditBoardDialog } from "@/app/edit-board-dialog";
import { DeleteBoardDialog } from "@/app/delete-board-dialog";
import { BoardMembersDialog } from "./board-members-dialog";
import {
  closestCenter,
  DndContext,
  DragOverlay,
  type DragEndEvent,
  type DragOverEvent,
  type DragStartEvent,
} from "@dnd-kit/core";
import {
  horizontalListSortingStrategy,
  SortableContext,
} from "@dnd-kit/sortable";
import { getColumnNeighbors, getMoveNeighbors } from "./board-ordering";

type BoardViewProps = {
  boardId: string;
};

type CardDropPreview = {
  cardId: string;
  columnId: string;
  afterCardId: string | null;
};

export function BoardView({ boardId }: BoardViewProps) {
  const [activeCardId, setActiveCardId] = useState<string | null>(null);
  const [cardDropPreview, setCardDropPreview] = useState<CardDropPreview | null>(null);
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

  function clearDragPreview() {
    setActiveCardId(null);
    setCardDropPreview(null);
  }

  function handleDragStart(event: DragStartEvent) {
    setActiveCardId(
      event.active.data.current?.type === "card" ? String(event.active.id) : null,
    );
  }

  function handleDragOver(event: DragOverEvent) {
    const { active, over } = event;
    if (!board || active.data.current?.type !== "card" || !over) {
      setCardDropPreview(null);
      return;
    }

    const cardId = String(active.id);
    const sourceColumnId = active.data.current.columnId;
    const columnId = over.data.current?.columnId;
    const overType = over.data.current?.type;
    if (
      typeof columnId !== "string"
      || sourceColumnId === columnId
      || (overType !== "card" && overType !== "column")
    ) {
      setCardDropPreview(null);
      return;
    }

    const neighbors = getMoveNeighbors(
      board, cardId, columnId, String(over.id), overType,
    );
    if (!neighbors) {
      setCardDropPreview(null);
      return;
    }

    setCardDropPreview((current) =>
      current?.cardId === cardId
      && current.columnId === columnId
      && current.afterCardId === neighbors.afterCardId
        ? current
        : { cardId, columnId, afterCardId: neighbors.afterCardId },
    );
  }

  function handleDragEnd(event: DragEndEvent) {
    const { active, over } = event;
    clearDragPreview();

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
    return <p className="text-sm text-slate-400">Loading board...</p>;
  }

  if (isError) {
    return <p className="text-sm text-red-300">Failed to load board.</p>;
  }

  if (!board) {
    return <p className="text-sm text-slate-400">Board data is unavailable.</p>;
  }

  const activeCard: Card | undefined = board.columns
    .flatMap((column) => column.cards)
    .find((card) => card.id === activeCardId);

  return (
    <section className="mx-auto w-full max-w-[1600px] px-4 py-8 sm:px-8">
      <div className="mb-8 flex flex-wrap items-center justify-between gap-4">
        <div>
          <Link href="/" className="text-sm text-slate-400 hover:text-cyan-300">
            Back to boards
          </Link>
          <h1 className="mt-1 text-3xl font-semibold tracking-tight text-slate-100">{board.name}</h1>
        </div>
        <div className="flex items-center gap-2">
          <BoardMembersDialog boardId={boardId} isOwner={board.isOwner} />
          {board.isOwner && <EditBoardDialog board={board} />}
          {board.isOwner && <DeleteBoardDialog board={board} returnToDashboard />}
          <CreateColumnDialog boardId={boardId} />
        </div>
      </div>

      {moveCardMutation.isError && (
        <p role="alert" className="mb-4 text-sm text-red-300">
          {moveCardMutation.error.message}
        </p>
      )}

      {reorderColumnMutation.isError && (
        <p role="alert" className="mb-4 text-sm text-red-300">
          {reorderColumnMutation.error.message}
        </p>
      )}

      <DndContext
        collisionDetection={closestCenter}
        onDragStart={handleDragStart}
        onDragOver={handleDragOver}
        onDragCancel={clearDragPreview}
        onDragEnd={handleDragEnd}
      >
        {board.columns.length === 0 ? (
          <p className="rounded-2xl border border-dashed border-white/15 bg-white/5 p-10 text-center text-sm text-slate-400 backdrop-blur-xl">
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
                  dropPreview={
                    cardDropPreview?.columnId === column.id && activeCard
                      ? { card: activeCard, afterCardId: cardDropPreview.afterCardId }
                      : null
                  }
                />
              ))}
            </div>
          </SortableContext>
        )}
        <DragOverlay dropAnimation={null}>
          {activeCard && (
            <div className="w-72 rotate-1 rounded-xl border border-cyan-300/50 bg-slate-900 p-4 text-slate-100 shadow-2xl shadow-black/50">
              <p className="text-xs font-medium uppercase tracking-wide text-cyan-300">Moving ticket</p>
              <p className="mt-1 font-medium">{activeCard.title}</p>
            </div>
          )}
        </DragOverlay>
      </DndContext>
    </section>
  );
}
