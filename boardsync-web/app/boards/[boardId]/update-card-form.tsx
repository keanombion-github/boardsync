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
        await Promise.all([
            queryClient.invalidateQueries({ queryKey: ["board", boardId] }),
            queryClient.invalidateQueries({ queryKey: ["card-activity", card.id] }),
        ]);

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
                className="mt-4 space-y-4 border-t border-white/10 pt-4"
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
                    className="w-full rounded-lg border border-white/15 bg-slate-950/60 px-3 py-2 text-sm text-slate-100 outline-none placeholder:text-slate-600 focus:border-cyan-300/60"
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
                    className="w-full resize-none rounded-lg border border-white/15 bg-slate-950/60 px-3 py-2 text-sm text-slate-100 outline-none placeholder:text-slate-600 focus:border-cyan-300/60"
                    />
                </div>

                {updateCardMutation.isError && (
                    <p role="alert" className="text-sm text-red-300">
                    {updateCardMutation.error.message}
                    </p>
                )}

                <button
                    type="submit"
                    disabled={
                    updateCardMutation.isPending || !title.trim()
                    }
                    className="w-full rounded-lg bg-cyan-300 px-3 py-2 text-sm font-semibold text-slate-950 hover:bg-cyan-200 disabled:cursor-not-allowed disabled:opacity-50"
                >
                    {updateCardMutation.isPending
                    ? "Updating..."
                    : "Update card"}
                </button>
            </form>
    )
}
