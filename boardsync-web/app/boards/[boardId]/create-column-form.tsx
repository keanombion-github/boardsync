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
    <form onSubmit={handleSubmit} className="mt-4 space-y-4 border-t border-white/10 pt-4">
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
          className="w-full rounded-lg border border-white/15 bg-slate-950/60 px-3 py-2 text-sm text-slate-100 outline-none placeholder:text-slate-600 focus:border-cyan-300/60"
          autoFocus
        />
      </div>

      {createColumnMutation.isError && (
        <p role="alert" className="text-sm text-red-300">
          {createColumnMutation.error.message}
        </p>
      )}

      <button
        type="submit"
        disabled={createColumnMutation.isPending || !name.trim()}
        className="w-full rounded-lg bg-cyan-300 px-3 py-2 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {createColumnMutation.isPending ? "Creating..." : "Create column"}
      </button>
    </form>
  );
}
