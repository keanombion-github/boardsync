"use client";

import { FormEvent, useState } from "react";
import { Dialog } from "@base-ui/react/dialog";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { Pencil, X } from "lucide-react";
import { updateBoard, type Board } from "@/lib/api/boards";

type EditBoardDialogProps = {
  board: Board;
};

export function EditBoardDialog({ board }: EditBoardDialogProps) {
  const [open, setOpen] = useState(false);
  const [name, setName] = useState(board.name);
  const queryClient = useQueryClient();

  function handleOpenChange(nextOpen: boolean) {
    if (nextOpen) setName(board.name);
    setOpen(nextOpen);
  }

  const mutation = useMutation({
    mutationFn: updateBoard,
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: ["boards"] }),
        queryClient.invalidateQueries({ queryKey: ["board", board.id] }),
      ]);
      setOpen(false);
    },
  });

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const trimmedName = name.trim();
    if (!trimmedName) return;
    mutation.mutate({ id: board.id, name: trimmedName });
  }

  return (
    <Dialog.Root open={open} onOpenChange={handleOpenChange}>
      <Dialog.Trigger
        aria-label={`Rename ${board.name}`}
        className="rounded-lg border border-white/10 bg-white/5 p-2 text-slate-300 hover:bg-white/10 hover:text-white"
      >
        <Pencil size={16} />
      </Dialog.Trigger>

      <Dialog.Portal>
        <Dialog.Backdrop className="fixed inset-0 z-40 bg-black/60 backdrop-blur-sm" />
        <Dialog.Viewport className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <Dialog.Popup className="w-full max-w-md rounded-2xl border border-white/15 bg-slate-900 p-6 text-slate-100 shadow-2xl">
            <div className="flex items-start justify-between gap-4">
              <div>
                <Dialog.Title className="text-lg font-semibold">
                  Rename board
                </Dialog.Title>
                <Dialog.Description className="mt-1 text-sm text-slate-400">
                  Use a name that makes this workflow easy to find.
                </Dialog.Description>
              </div>
              <Dialog.Close
                aria-label="Close rename-board dialog"
                className="rounded-md p-1 text-slate-400 hover:bg-white/10 hover:text-white"
              >
                <X size={18} />
              </Dialog.Close>
            </div>

            <form onSubmit={handleSubmit} className="mt-5 space-y-4">
              <label className="block text-sm font-medium">
                Board name
                <input
                  value={name}
                  onChange={(event) => setName(event.target.value)}
                  maxLength={200}
                  autoFocus
                  className="mt-2 w-full rounded-lg border border-white/15 bg-white/5 px-3 py-2 outline-none focus:border-cyan-400"
                />
              </label>

              {mutation.isError && (
                <p role="alert" className="text-sm text-red-400">
                  {mutation.error.message}
                </p>
              )}

              <div className="flex justify-end gap-2">
                <Dialog.Close
                  disabled={mutation.isPending}
                  className="rounded-lg border border-white/15 px-3 py-2 text-sm disabled:opacity-50"
                >
                  Cancel
                </Dialog.Close>
                <button
                  type="submit"
                  disabled={mutation.isPending || !name.trim()}
                  className="rounded-lg bg-cyan-400 px-3 py-2 text-sm font-medium text-slate-950 hover:bg-cyan-300 disabled:opacity-50"
                >
                  {mutation.isPending ? "Saving..." : "Save"}
                </button>
              </div>
            </form>
          </Dialog.Popup>
        </Dialog.Viewport>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
