"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Dialog } from "@base-ui/react/dialog";
import { Plus, X } from "lucide-react";
import { CreateBoardForm } from "./create-board-form";

export function CreateBoardDialog() {
  const [open, setOpen] = useState(false);
  const router = useRouter();

  function handleCreated(boardId: string) {
    setOpen(false);
    router.push(`/boards/${boardId}`);
  }

  return (
    <Dialog.Root open={open} onOpenChange={setOpen}>
      <Dialog.Trigger className="inline-flex items-center gap-2 rounded-lg bg-cyan-300 px-3 py-2 text-sm font-semibold text-slate-950 hover:bg-cyan-200">
        <Plus size={16} />
        New board
      </Dialog.Trigger>

      <Dialog.Portal>
        <Dialog.Backdrop className="fixed inset-0 z-40 bg-black/60 backdrop-blur-sm" />

        <Dialog.Viewport className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <Dialog.Popup className="w-full max-w-md rounded-2xl border border-white/15 bg-slate-900/95 p-6 text-slate-100 shadow-2xl backdrop-blur-2xl">
            <div className="flex items-start justify-between gap-4">
              <div>
                <Dialog.Title className="text-lg font-semibold">
                  Create board
                </Dialog.Title>
                <Dialog.Description className="text-sm text-slate-400">
                  Start a workspace for a project or workflow.
                </Dialog.Description>
              </div>

              <Dialog.Close
                aria-label="Close create-board dialog"
                className="rounded-md p-1 text-slate-400 hover:bg-white/10 hover:text-white"
              >
                <X size={18} />
              </Dialog.Close>
            </div>

            <CreateBoardForm onCreated={handleCreated} />
          </Dialog.Popup>
        </Dialog.Viewport>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
