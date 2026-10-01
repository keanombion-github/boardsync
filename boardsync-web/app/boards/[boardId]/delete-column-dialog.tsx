"use client";

import { useState } from "react";
import { AlertDialog } from "@base-ui/react/alert-dialog";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Trash2 } from "lucide-react";
import { deleteColumn, type BoardColumn } from "@/lib/api/boards";

type DeleteColumnDialogProps = {
  boardId: string;
  column: BoardColumn;
};

export function DeleteColumnDialog({
  boardId,
  column,
}: DeleteColumnDialogProps) {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();

  const deleteColumnMutation = useMutation({
    mutationFn: () => deleteColumn(boardId, column.id),

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
        aria-label={`Delete ${column.name} column`}
        className="rounded-md p-1 text-slate-400 hover:bg-red-500/15 hover:text-red-300"
      >
        <Trash2 size={16} />
      </AlertDialog.Trigger>

      <AlertDialog.Portal>
        <AlertDialog.Backdrop className="fixed inset-0 z-40 bg-black/60 backdrop-blur-sm" />

        <AlertDialog.Viewport className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <AlertDialog.Popup className="w-full max-w-md rounded-2xl border border-white/15 bg-slate-900 p-6 text-slate-100 shadow-2xl">
            <AlertDialog.Title className="text-lg font-semibold">
              Delete column
            </AlertDialog.Title>

            <AlertDialog.Description className="mt-2 text-sm text-slate-400">
              Delete &quot;{column.name}&quot; and its {column.cards.length}{" "}
              {column.cards.length === 1 ? "card" : "cards"}? This cannot be
              undone.
            </AlertDialog.Description>

            {deleteColumnMutation.isError && (
              <p role="alert" className="mt-3 text-sm text-red-300">
                {deleteColumnMutation.error.message}
              </p>
            )}

            <div className="mt-6 flex justify-end gap-2">
              <AlertDialog.Close
                disabled={deleteColumnMutation.isPending}
                className="rounded-lg border border-white/15 px-3 py-2 text-sm disabled:opacity-50"
              >
                Cancel
              </AlertDialog.Close>

              <button
                type="button"
                onClick={() => deleteColumnMutation.mutate()}
                disabled={deleteColumnMutation.isPending}
                className="rounded-lg bg-red-600 px-3 py-2 text-sm text-white hover:bg-red-500 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {deleteColumnMutation.isPending ? "Deleting..." : "Delete"}
              </button>
            </div>
          </AlertDialog.Popup>
        </AlertDialog.Viewport>
      </AlertDialog.Portal>
    </AlertDialog.Root>
  );
}
