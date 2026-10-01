"use client";

import { useState, type FormEvent } from "react";
import { Dialog } from "@base-ui/react/dialog";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { Users, X } from "lucide-react";
import { addBoardMember, getBoardMembers, removeBoardMember } from "@/lib/api/boards";

export function BoardMembersDialog({ boardId, isOwner }: { boardId: string; isOwner: boolean }) {
  const [open, setOpen] = useState(false);
  const [email, setEmail] = useState("");
  const [confirmRemoveId, setConfirmRemoveId] = useState<string | null>(null);
  const queryClient = useQueryClient();
  const queryKey = ["board-members", boardId];
  const { data: members, isPending, isError, error } = useQuery({
    queryKey,
    queryFn: () => getBoardMembers(boardId),
    enabled: open,
  });
  const addMutation = useMutation({
    mutationFn: (memberEmail: string) => addBoardMember(boardId, memberEmail),
    onSuccess: async () => {
      setEmail("");
      await queryClient.invalidateQueries({ queryKey });
    },
  });
  const removeMutation = useMutation({
    mutationFn: (memberId: string) => removeBoardMember(boardId, memberId),
    onSuccess: async () => {
      setConfirmRemoveId(null);
      await Promise.all([
        queryClient.invalidateQueries({ queryKey }),
        queryClient.invalidateQueries({ queryKey: ["card-activity"] }),
      ]);
    },
  });

  function invite(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (email.trim()) addMutation.mutate(email.trim());
  }

  return (
    <Dialog.Root open={open} onOpenChange={setOpen}>
      <Dialog.Trigger className="inline-flex items-center gap-2 rounded-lg border border-white/10 bg-white/5 px-3 py-2 text-sm text-slate-300 hover:bg-white/10">
        <Users size={16} /> Members
      </Dialog.Trigger>
      <Dialog.Portal>
        <Dialog.Backdrop className="fixed inset-0 z-40 bg-black/60 backdrop-blur-sm" />
        <Dialog.Viewport className="fixed inset-0 z-50 flex items-center justify-center overflow-y-auto p-4">
          <Dialog.Popup className="my-auto w-full max-w-md rounded-2xl border border-white/15 bg-slate-900 p-6 text-slate-100 shadow-2xl">
            <div className="flex items-start justify-between gap-4">
              <div>
                <Dialog.Title className="text-lg font-semibold">Board members</Dialog.Title>
                <Dialog.Description className="mt-1 text-sm text-slate-400">Members can work on tickets. Only the owner can rename or delete this board.</Dialog.Description>
              </div>
              <Dialog.Close aria-label="Close members" className="rounded p-1 text-slate-400 hover:text-white"><X size={18} /></Dialog.Close>
            </div>
            {isPending && <p className="mt-5 text-sm text-slate-400">Loading members...</p>}
            {isError && <p role="alert" className="mt-5 text-sm text-red-300">{error.message}</p>}
            <ul className="mt-5 space-y-2">
              {members?.map((member) => (
                <li key={member.id} className="flex items-center justify-between gap-3 rounded-lg border border-white/10 bg-white/5 px-3 py-2 text-sm">
                  <div className="min-w-0">
                    <p className="truncate font-medium">{member.displayName}{member.isOwner ? " · owner" : ""}</p>
                    <p className="truncate text-xs text-slate-400">{member.email}</p>
                  </div>
                  {isOwner && !member.isOwner && (
                    confirmRemoveId === member.id ? (
                      <div className="flex gap-2">
                        <button type="button" disabled={removeMutation.isPending} onClick={() => removeMutation.mutate(member.id)} className="text-xs text-red-300 hover:underline">Confirm</button>
                        <button type="button" onClick={() => setConfirmRemoveId(null)} className="text-xs text-slate-400 hover:underline">Cancel</button>
                      </div>
                    ) : (
                      <button type="button" onClick={() => setConfirmRemoveId(member.id)} className="text-xs text-slate-400 hover:text-red-300">Remove</button>
                    )
                  )}
                </li>
              ))}
            </ul>
            {removeMutation.isError && <p role="alert" className="mt-3 text-sm text-red-300">{removeMutation.error.message}</p>}
            {isOwner && (
              <form onSubmit={invite} className="mt-6 space-y-3 border-t border-white/10 pt-5">
                <label htmlFor={`member-email-${boardId}`} className="block text-sm font-medium">Add a registered user</label>
                <div className="flex gap-2">
                  <input id={`member-email-${boardId}`} type="email" required value={email} onChange={(event) => setEmail(event.target.value)} placeholder="teammate@example.com" disabled={addMutation.isPending} className="min-w-0 flex-1 rounded-lg border border-white/15 bg-slate-950/60 px-3 py-2 text-sm outline-none focus:border-cyan-300/60" />
                  <button type="submit" disabled={addMutation.isPending} className="rounded-lg bg-cyan-300 px-3 py-2 text-sm font-semibold text-slate-950 disabled:opacity-50">Add</button>
                </div>
                <p className="text-xs text-slate-500">The person must create a Boardsync account first.</p>
                {addMutation.isError && <p role="alert" className="text-sm text-red-300">{addMutation.error.message}</p>}
              </form>
            )}
          </Dialog.Popup>
        </Dialog.Viewport>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
