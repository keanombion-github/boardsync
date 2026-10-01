"use client";

import { useState } from "react";
import { AlertDialog } from "@base-ui/react/alert-dialog";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Trash2 } from "lucide-react";
import { deleteCard, type Card } from "@/lib/api/boards";

type DeleteCardDialogProps = {
  boardId: string;
  card: Card;
};

export function DeleteCardDialog({ boardId, card }: DeleteCardDialogProps) {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();

  const mutation = useMutation({
    mutationFn: () => deleteCard(card.id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["board", boardId] });
      setOpen(false);
    },
  });

  return (
    <AlertDialog.Root open={open} onOpenChange={setOpen}>
      <AlertDialog.Trigger
        aria-label={`Delete ${card.title}`}
        className="rounded-md p-1 text-slate-400 hover:bg-red-500/15 hover:text-red-300"
      >
        <Trash2 size={18} />
      </AlertDialog.Trigger>

      <AlertDialog.Portal>
        <AlertDialog.Backdrop className="fixed inset-0 z-40 bg-black/60 backdrop-blur-sm" />
        <AlertDialog.Viewport className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <AlertDialog.Popup className="w-full max-w-md rounded-2xl border border-white/15 bg-slate-900 p-6 text-slate-100 shadow-2xl">
            <AlertDialog.Title className="text-lg font-semibold">
              Delete card
            </AlertDialog.Title>
            <AlertDialog.Description className="mt-2 text-sm text-slate-400">
              Delete &quot;{card.title}&quot;? This cannot be undone.
            </AlertDialog.Description>

            {mutation.isError && (
              <p role="alert" className="mt-3 text-sm text-red-300">
                {mutation.error.message}
              </p>
            )}

            <div className="mt-6 flex justify-end gap-2">
              <AlertDialog.Close
                disabled={mutation.isPending}
                className="rounded-lg border border-white/15 px-3 py-2 text-sm disabled:opacity-50"
              >
                Cancel
              </AlertDialog.Close>
              <button
                type="button"
                onClick={() => mutation.mutate()}
                disabled={mutation.isPending}
                className="rounded-lg bg-red-600 px-3 py-2 text-sm font-medium text-white hover:bg-red-500 disabled:opacity-50"
              >
                {mutation.isPending ? "Deleting..." : "Delete"}
              </button>
            </div>
          </AlertDialog.Popup>
        </AlertDialog.Viewport>
      </AlertDialog.Portal>
    </AlertDialog.Root>
  );
}
