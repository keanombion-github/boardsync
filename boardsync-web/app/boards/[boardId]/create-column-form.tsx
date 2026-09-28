"use client";

import { useState, type FormEvent } from "react";
import {
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import { createColumn } from "@/lib/api/boards";

type CreateColumnFormProps = {
  boardId: string;
  onCreated?: () => void;
};

export function CreateColumnForm({
  boardId,
  onCreated,
}: CreateColumnFormProps) {
  const [name, setName] = useState("");
  const queryClient = useQueryClient();

  const createColumnMutation = useMutation({
    mutationFn: createColumn,

    onSuccess: async () => {
      setName("");

      await queryClient.invalidateQueries({
        queryKey: ["board", boardId],
      });

      onCreated?.();
    },
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const trimmedName = name.trim();

    if (!trimmedName) {
      return;
    }

    createColumnMutation.mutate({
      boardId,
      name: trimmedName,
    });
  }

  return (
    <form onSubmit={handleSubmit} className="mt-4 space-y-4 border-t pt-4">
      <div>
        <label
          htmlFor="column-name"
          className="mb-1 block text-sm font-medium"
        >
          Column name
        </label>

        <input
          id="column-name"
          type="text"
          value={name}
          onChange={(event) => setName(event.target.value)}
          placeholder="For example: Done"
          maxLength={200}
          disabled={createColumnMutation.isPending}
          className="w-full rounded-md border bg-white px-3 py-2 text-sm text-gray-950"
          autoFocus
        />
      </div>

      {createColumnMutation.isError && (
        <p role="alert" className="text-sm text-red-600">
          {createColumnMutation.error.message}
        </p>
      )}

      <button
        type="submit"
        disabled={createColumnMutation.isPending || !name.trim()}
        className="w-full rounded-md bg-black px-3 py-2 text-sm text-white disabled:cursor-not-allowed disabled:opacity-50"
      >
        {createColumnMutation.isPending ? "Creating..." : "Create column"}
      </button>
    </form>
  );
}
