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
        className="rounded-md p-1 hover:bg-gray-200"
      >
        <Plus size={18} />
      </Dialog.Trigger>

      <Dialog.Portal>
        <Dialog.Backdrop className="fixed inset-0 bg-black/50 backdrop-blur-sm" />

        <Dialog.Viewport className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Popup className="w-full max-w-md rounded-xl border border-white/20 bg-white/90 p-6 shadow-2xl backdrop-blur-xl">
            <div className="mb-4 flex items-start justify-between gap-4">
              <div>
                <Dialog.Title className="text-lg font-semibold">
                  Create card
                </Dialog.Title>

                <Dialog.Description className="text-sm text-gray-600">
                  Add a card to this column.
                </Dialog.Description>
              </div>

              <Dialog.Close
                aria-label="Close create-card dialog"
                className="rounded-md p-1 hover:bg-gray-200"
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