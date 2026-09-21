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
      className="mt-4 space-y-3 border-t pt-4"
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
          className="w-full rounded-md border bg-white px-3 py-2 text-sm"
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
          className="w-full resize-none rounded-md border bg-white px-3 py-2 text-sm"
        />
      </div>

      {createCardMutation.isError && (
        <p role="alert" className="text-sm text-red-600">
          {createCardMutation.error.message}
        </p>
      )}

      <button
        type="submit"
        disabled={
          createCardMutation.isPending || !title.trim()
        }
        className="w-full rounded-md bg-black px-3 py-2 text-sm text-white disabled:cursor-not-allowed disabled:opacity-50"
      >
        {createCardMutation.isPending
          ? "Creating..."
          : "Add card"}
      </button>
    </form>
  );
}