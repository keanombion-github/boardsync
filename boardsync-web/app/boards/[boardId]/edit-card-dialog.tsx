"use client";

import { useState } from "react";
import { Dialog } from "@base-ui/react/dialog";
import { Pencil, X } from "lucide-react";
import { UpdateCardForm } from "./update-card-form";
import type { Card } from "@/lib/api/boards";

type EditCardDialogProps = {
  boardId: string;
  card: Card;
};

export function EditCardDialog({
  boardId,
  card,
}: EditCardDialogProps) {
  const [open, setOpen] = useState(false);
  return (
        <Dialog.Root open={open} onOpenChange={setOpen}>
          <Dialog.Trigger
            aria-label="Update card"
            className="rounded-md p-1 hover:bg-gray-200"
          >
            <Pencil size={18} />
          </Dialog.Trigger>
    
          <Dialog.Portal>
            <Dialog.Backdrop className="fixed inset-0 bg-black/50 backdrop-blur-sm" />
    
            <Dialog.Viewport className="fixed inset-0 flex items-center justify-center p-4">
              <Dialog.Popup className="w-full max-w-md rounded-xl border border-white/20 bg-white/90 p-6 shadow-2xl backdrop-blur-xl">
                <div className="mb-4 flex items-start justify-between gap-4">
                  <div>
                    <Dialog.Title className="text-lg font-semibold">
                     Update Card
                    </Dialog.Title>
    
                    <Dialog.Description className="text-sm text-gray-600">
                        Update this card&apos;s title or description.
                    </Dialog.Description>
                  </div>
    
                  <Dialog.Close
                    aria-label="Close update-card dialog"
                    className="rounded-md p-1 hover:bg-gray-200"
                  >
                    <X size={18} />
                  </Dialog.Close>
                </div>
    
               <UpdateCardForm
                boardId={boardId}
                card={card}
                onUpdated={() => setOpen(false)}
                />
              </Dialog.Popup>
            </Dialog.Viewport>
          </Dialog.Portal>
        </Dialog.Root>
  )
}