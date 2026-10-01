"use client";

import { useState } from "react";
import { Dialog } from "@base-ui/react/dialog";
import { Plus, X } from "lucide-react";
import { CreateCardForm } from "./create-card-form";

type CreateCardDialogProps = {
  boardId: string;
  columnId: string;
};

export function CreateCardDialog({
  boardId,
  columnId,
}: CreateCardDialogProps) {
  const [open, setOpen] = useState(false);

  return (
    <Dialog.Root open={open} onOpenChange={setOpen}>
      <Dialog.Trigger
        aria-label="Create card"
        className="rounded-md p-1 text-slate-400 hover:bg-white/10 hover:text-cyan-300"
      >
        <Plus size={18} />
      </Dialog.Trigger>

      <Dialog.Portal>
        <Dialog.Backdrop className="fixed inset-0 z-40 bg-black/60 backdrop-blur-sm" />

        <Dialog.Viewport className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <Dialog.Popup className="w-full max-w-md rounded-2xl border border-white/15 bg-slate-900/95 p-6 text-slate-100 shadow-2xl backdrop-blur-2xl">
            <div className="mb-4 flex items-start justify-between gap-4">
              <div>
                <Dialog.Title className="text-lg font-semibold">
                  Create card
                </Dialog.Title>

                <Dialog.Description className="text-sm text-slate-400">
                  Add a card to this column.
                </Dialog.Description>
              </div>

              <Dialog.Close
                aria-label="Close create-card dialog"
                className="rounded-md p-1 text-slate-400 hover:bg-white/10 hover:text-white"
              >
                <X size={18} />
              </Dialog.Close>
            </div>

            <CreateCardForm
              boardId={boardId}
              columnId={columnId}
              onCreated={() => setOpen(false)}
            />
          </Dialog.Popup>
        </Dialog.Viewport>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
