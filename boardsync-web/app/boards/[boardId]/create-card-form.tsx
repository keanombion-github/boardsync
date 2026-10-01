"use client";

import { useState, type FormEvent } from "react";
import {
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import { createCard } from "@/lib/api/boards";

type CreateCardFormProps = {
  boardId: string;
  columnId: string;
  onCreated?: () => void;
};

export function CreateCardForm({
  boardId,
  columnId,
  onCreated,
}: CreateCardFormProps) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");

  const queryClient = useQueryClient();

  const createCardMutation = useMutation({
    mutationFn: createCard,

    onSuccess: async () => {
      setTitle("");
      setDescription("");

      await queryClient.invalidateQueries({
        queryKey: ["board", boardId],
      });

      onCreated?.();
    },
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    const trimmedTitle = title.trim();
    const trimmedDescription = description.trim();

    if (!trimmedTitle) {
      return;
    }

    createCardMutation.mutate({
      title: trimmedTitle,
      description: trimmedDescription || null,
      columnId,
    });
  }

  return (
    <form
      onSubmit={handleSubmit}
      className="mt-4 space-y-4 border-t border-white/10 pt-4"
    >
      <div>
        <label
          htmlFor={`card-title-${columnId}`}
          className="mb-1 block text-sm font-medium"
        >
          Card title
        </label>

        <input
          id={`card-title-${columnId}`}
          type="text"
          value={title}
          onChange={(event) => setTitle(event.target.value)}
          placeholder="Enter a card title"
          disabled={createCardMutation.isPending}
          className="w-full rounded-lg border border-white/15 bg-slate-950/60 px-3 py-2 text-sm text-slate-100 outline-none placeholder:text-slate-600 focus:border-cyan-300/60"
        />
      </div>

      <div>
        <label
          htmlFor={`card-description-${columnId}`}
          className="mb-1 block text-sm font-medium"
        >
          Description
        </label>

        <textarea
          id={`card-description-${columnId}`}
          value={description}
          onChange={(event) =>
            setDescription(event.target.value)
          }
          placeholder="Optional description"
          disabled={createCardMutation.isPending}
          rows={3}
          className="w-full resize-none rounded-lg border border-white/15 bg-slate-950/60 px-3 py-2 text-sm text-slate-100 outline-none placeholder:text-slate-600 focus:border-cyan-300/60"
        />
      </div>

      {createCardMutation.isError && (
        <p role="alert" className="text-sm text-red-300">
          {createCardMutation.error.message}
        </p>
      )}

      <button
        type="submit"
        disabled={
          createCardMutation.isPending || !title.trim()
        }
        className="w-full rounded-lg bg-cyan-300 px-3 py-2 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {createCardMutation.isPending
          ? "Creating..."
          : "Add card"}
      </button>
    </form>
  );
}
