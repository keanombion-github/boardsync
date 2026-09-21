"use client";

import { useState, type FormEvent } from "react";
import {
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import type { Card } from "@/lib/api/boards";
import { updateCard } from "@/lib/api/boards";

type UpdateCardFormProps = {
  boardId: string;
  card: Card;
  onUpdated?: () => void;
};

export function UpdateCardForm({ 
    boardId, card, onUpdated 
}: UpdateCardFormProps) {
    const [title, setTitle] = useState(card.title);
    const [description, setDescription] = useState(card.description ?? "");

    const queryClient = useQueryClient();

    const updateCardMutation = useMutation({
    mutationFn: updateCard,

    onSuccess: async () => {
        await queryClient.invalidateQueries({
        queryKey: ["board", boardId],
        });

        onUpdated?.();
    },
    });

    function handleSubmit(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();

        const trimmedTitle = title.trim();
        const trimmedDescription = description.trim();

        if (!trimmedTitle) {
            return;
        }

        updateCardMutation.mutate({
            id: card.id,
            title: trimmedTitle,
            description: trimmedDescription || null,
        });
    }

    return (
            <form
                onSubmit={handleSubmit}
                className="mt-4 space-y-3 border-t pt-4"
                >
                <div>
                    <label
                    htmlFor={`card-title-${card.id}`}
                    className="mb-1 block text-sm font-medium"
                    >
                    Card title
                    </label>

                    <input
                    id={`card-title-${card.id}`}
                    type="text"
                    value={title}
                    onChange={(event) => setTitle(event.target.value)}
                    placeholder="Enter a card title"
                    disabled={updateCardMutation.isPending}
                    className="w-full rounded-md border bg-white px-3 py-2 text-sm"
                    />
                </div>

                <div>
                    <label
                    htmlFor={`card-description-${card.id}`}
                    className="mb-1 block text-sm font-medium"
                    >
                    Description
                    </label>

                    <textarea
                    id={`card-description-${card.id}`}
                    value={description}
                    onChange={(event) =>
                        setDescription(event.target.value)
                    }
                    placeholder="Optional description"
                    disabled={updateCardMutation.isPending}
                    rows={3}
                    className="w-full resize-none rounded-md border bg-white px-3 py-2 text-sm"
                    />
                </div>

                {updateCardMutation.isError && (
                    <p role="alert" className="text-sm text-red-600">
                    {updateCardMutation.error.message}
                    </p>
                )}

                <button
                    type="submit"
                    disabled={
                    updateCardMutation.isPending || !title.trim()
                    }
                    className="w-full rounded-md bg-black px-3 py-2 text-sm text-white disabled:cursor-not-allowed disabled:opacity-50"
                >
                    {updateCardMutation.isPending
                    ? "Updating..."
                    : "Update card"}
                </button>
            </form>
    )
}