"use client";

import { useState, type FormEvent } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createBoard } from "@/lib/api/boards";

type CreateBoardFormProps = {
  onCreated?: (boardId: string) => void;
};

export function CreateBoardForm({ onCreated }: CreateBoardFormProps) {
  const [name, setName] = useState("");
  const queryClient = useQueryClient();

  const createBoardMutation = useMutation({
    mutationFn: createBoard,

    onSuccess: async (result) => {
      setName("");

      await queryClient.invalidateQueries({
        queryKey: ["boards"],
      });

      onCreated?.(result.id);
    },
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const trimmedName = name.trim();

    if (!trimmedName) {
      return;
    }

    createBoardMutation.mutate({
      name: trimmedName,
    });
  }

  return (
    <form onSubmit={handleSubmit} className="mt-4 space-y-4 border-t border-white/10 pt-4">
      <div>
        <label htmlFor="board-name" className="mb-1 block text-sm font-medium">
          Board name
        </label>

        <input
          id="board-name"
          type="text"
          value={name}
          onChange={(event) => setName(event.target.value)}
          placeholder="For example: Product roadmap"
          maxLength={200}
          disabled={createBoardMutation.isPending}
          className="w-full rounded-lg border border-white/15 bg-slate-950/60 px-3 py-2 text-sm text-slate-100 outline-none placeholder:text-slate-600 focus:border-cyan-300/60"
          autoFocus
        />
      </div>

      {createBoardMutation.isError && (
        <p role="alert" className="text-sm text-red-300">
          {createBoardMutation.error.message}
        </p>
      )}

      <button
        type="submit"
        disabled={createBoardMutation.isPending || !name.trim()}
        className="w-full rounded-lg bg-cyan-300 px-3 py-2 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {createBoardMutation.isPending ? "Creating..." : "Create board"}
      </button>
    </form>
  );
}
