"use client";

import { useState } from "react";
import { Dialog } from "@base-ui/react/dialog";
import { Plus, X } from "lucide-react";
import { CreateColumnForm } from "./create-column-form";

type CreateColumnDialogProps = {
  boardId: string;
};

export function CreateColumnDialog({ boardId }: CreateColumnDialogProps) {
  const [open, setOpen] = useState(false);

  return (
    <Dialog.Root open={open} onOpenChange={setOpen}>
      <Dialog.Trigger className="inline-flex items-center gap-2 rounded-md bg-black px-3 py-2 text-sm text-white hover:bg-gray-800">
        <Plus size={16} />
        Add column
      </Dialog.Trigger>

      <Dialog.Portal>
        <Dialog.Backdrop className="fixed inset-0 bg-black/50 backdrop-blur-sm" />

        <Dialog.Viewport className="fixed inset-0 flex items-center justify-center p-4">
          <Dialog.Popup className="w-full max-w-md rounded-xl border border-white/20 bg-white/95 p-6 text-gray-950 shadow-2xl backdrop-blur-xl">
            <div className="flex items-start justify-between gap-4">
              <div>
                <Dialog.Title className="text-lg font-semibold">
                  Create column
                </Dialog.Title>
                <Dialog.Description className="text-sm text-gray-600">
                  Add another workflow stage to this board.
                </Dialog.Description>
              </div>

              <Dialog.Close
                aria-label="Close create-column dialog"
                className="rounded-md p-1 hover:bg-gray-200"
              >
                <X size={18} />
              </Dialog.Close>
            </div>

            <CreateColumnForm
              boardId={boardId}
              onCreated={() => setOpen(false)}
            />
          </Dialog.Popup>
        </Dialog.Viewport>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
