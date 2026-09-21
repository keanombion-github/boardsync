"use client";

import { useState } from "react";
import { AlertDialog } from "@base-ui/react/alert-dialog";
import {
  useMutation,
  useQueryClient,
} from "@tanstack/react-query";
import { Trash } from "lucide-react";
import { deleteCard, type Card } from "@/lib/api/boards";

type DeleteCardDialogProps = {
  boardId: string;
  card: Card;
};

export function DeleteCardDialog({
  boardId,
  card,
}: DeleteCardDialogProps) {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();
  const deleteCardMutation = useMutation({
    mutationFn: () => deleteCard(card.id),

    onSuccess: async () => {
        await queryClient.invalidateQueries({
        queryKey: ["board", boardId],
        });

        setOpen(false);
    },
});
  return (
    <AlertDialog.Root open={open} onOpenChange={setOpen}>
      <AlertDialog.Trigger
        aria-label={`Delete ${card.title}`}
        className="rounded-md p-1 hover:bg-gray-200"
      >
        <Trash size={18} />
      </AlertDialog.Trigger>
      <AlertDialog.Portal>
        <AlertDialog.Backdrop className="fixed inset-0 bg-black/50 backdrop-blur-sm" />
        <AlertDialog.Viewport className="fixed left-1/2 top-1/2 max-h-[80vh] w-[90vw] max-w-md -translate-x-1/2 -translate-y-1/2 rounded-lg bg-white p-6 shadow-lg">
            <AlertDialog.Popup className="w-full max-w-md rounded-lg bg-white p-6 shadow-lg">
                <AlertDialog.Title className="text-lg font-semibold text-gray-900">
                    Delete Card
                </AlertDialog.Title>
                <AlertDialog.Description>
                Are you sure you want to delete “{card.title}”? This action cannot be undone.
                </AlertDialog.Description>
                <div className="mt-4 flex justify-end gap-2">
                   <AlertDialog.Close disabled={deleteCardMutation.isPending}>
                        Cancel
                    </AlertDialog.Close>
                    <button
                    type="button"
                    onClick={() => deleteCardMutation.mutate()}
                    disabled={deleteCardMutation.isPending}
                    >
                        {deleteCardMutation.isPending ? "Deleting..." : "Delete"}
                    </button>
                </div>

                {deleteCardMutation.isError && (
                    <p role="alert" className="mt-3 text-sm text-red-600">
                        {deleteCardMutation.error.message}
                    </p>
                )}
            </AlertDialog.Popup>
        </AlertDialog.Viewport>
      </AlertDialog.Portal>
    </AlertDialog.Root>
  );
}