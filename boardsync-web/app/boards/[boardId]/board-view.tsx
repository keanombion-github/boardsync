"use client";

import { useQuery } from "@tanstack/react-query";
import { getBoardById } from "@/lib/api/boards";
import BoardColumn from "./board-column";

type BoardViewProps = {
  boardId: string;
};

export function BoardView({ boardId }: BoardViewProps) {
  const {
    data: board,
    isLoading,
    isError,
  } = useQuery({
    queryKey: ["board", boardId],
    queryFn: () => getBoardById(boardId),
  });

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
  <section>
    <h1 className="mb-6 text-2xl font-bold">{board.name}</h1>

    <div className="flex items-start gap-4 overflow-x-auto">
      {board.columns.map((column) => (
        <BoardColumn key={column.id} column={column} boardId={boardId} />
      ))}
    </div>
  </section>
);
}