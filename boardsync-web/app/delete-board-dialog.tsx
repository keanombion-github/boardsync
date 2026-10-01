"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { AlertDialog } from "@base-ui/react/alert-dialog";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Trash2 } from "lucide-react";
import { deleteBoard, type Board } from "@/lib/api/boards";

type DeleteBoardDialogProps = {
  board: Board;
  returnToDashboard?: boolean;
};

export function DeleteBoardDialog({
  board,
  returnToDashboard = false,
}: DeleteBoardDialogProps) {
  const [open, setOpen] = useState(false);
  const queryClient = useQueryClient();
  const router = useRouter();

  const mutation = useMutation({
    mutationFn: () => deleteBoard(board.id),
    onSuccess: async () => {
      queryClient.removeQueries({ queryKey: ["board", board.id] });
      await queryClient.invalidateQueries({ queryKey: ["boards"] });
      setOpen(false);
      if (returnToDashboard) router.replace("/");
    },
  });

  return (
    <AlertDialog.Root open={open} onOpenChange={setOpen}>
      <AlertDialog.Trigger
        aria-label={`Delete ${board.name}`}
        className="rounded-lg border border-white/10 bg-white/5 p-2 text-slate-300 hover:bg-red-500/15 hover:text-red-300"
      >
        <Trash2 size={16} />
      </AlertDialog.Trigger>

      <AlertDialog.Portal>
        <AlertDialog.Backdrop className="fixed inset-0 z-40 bg-black/60 backdrop-blur-sm" />
        <AlertDialog.Viewport className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <AlertDialog.Popup className="w-full max-w-md rounded-2xl border border-white/15 bg-slate-900 p-6 text-slate-100 shadow-2xl">
            <AlertDialog.Title className="text-lg font-semibold">
              Delete board
            </AlertDialog.Title>
            <AlertDialog.Description className="mt-2 text-sm text-slate-400">
              Delete &quot;{board.name}&quot;, including all of its columns and
              cards? This cannot be undone.
            </AlertDialog.Description>

            {mutation.isError && (
              <p role="alert" className="mt-3 text-sm text-red-400">
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
                {mutation.isPending ? "Deleting..." : "Delete board"}
              </button>
            </div>
          </AlertDialog.Popup>
        </AlertDialog.Viewport>
      </AlertDialog.Portal>
    </AlertDialog.Root>
  );
}
