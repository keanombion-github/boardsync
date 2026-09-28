"use client";

import { useState, type FormEvent } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createBoard } from "@/lib/api/boards";

type CreateBoardFormProps = {
  ownerId: string;
  onCreated?: (boardId: string) => void;
};

export function CreateBoardForm({
  ownerId,
  onCreated,
}: CreateBoardFormProps) {
  const [name, setName] = useState("");
  const queryClient = useQueryClient();

  const createBoardMutation = useMutation({
    mutationFn: createBoard,

    onSuccess: async (result) => {
      setName("");

      await queryClient.invalidateQueries({
        queryKey: ["boards", ownerId],
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
      ownerId,
    });
  }

  return (
    <form onSubmit={handleSubmit} className="mt-4 space-y-4 border-t pt-4">
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
          className="w-full rounded-md border bg-white px-3 py-2 text-sm text-gray-950"
          autoFocus
        />
      </div>

      {createBoardMutation.isError && (
        <p role="alert" className="text-sm text-red-600">
          {createBoardMutation.error.message}
        </p>
      )}

      <button
        type="submit"
        disabled={createBoardMutation.isPending || !name.trim()}
        className="w-full rounded-md bg-black px-3 py-2 text-sm text-white disabled:cursor-not-allowed disabled:opacity-50"
      >
        {createBoardMutation.isPending ? "Creating..." : "Create board"}
      </button>
    </form>
  );
}
