"use client";

import { useState, type FormEvent } from "react";
import { Dialog } from "@base-ui/react/dialog";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { ExternalLink, Paperclip, Trash2, X } from "lucide-react";
import { UpdateCardForm } from "./update-card-form";
import {
  addCardAttachment,
  addCardComment,
  assignCard,
  deleteCard,
  deleteCardAttachment,
  getBoardMembers,
  getCardActivity,
  toggleCardReaction,
  type Card,
} from "@/lib/api/boards";

const reactions = ["👍", "❤️", "🎉", "👀"];

export function CardActivityDialog({ boardId, card, open, onOpenChange }: { boardId: string; card: Card; open: boolean; onOpenChange: (open: boolean) => void }) {
  const [confirmDelete, setConfirmDelete] = useState(false);
  const [comment, setComment] = useState("");
  const [attachmentLabel, setAttachmentLabel] = useState("");
  const [attachmentUrl, setAttachmentUrl] = useState("");
  function handleOpenChange(nextOpen: boolean) {
    if (!nextOpen) setConfirmDelete(false);
    onOpenChange(nextOpen);
  }
  const queryClient = useQueryClient();
  const queryKey = ["card-activity", card.id];
  const { data, isPending, isError, error } = useQuery({
    queryKey,
    queryFn: () => getCardActivity(card.id),
    enabled: open,
  });
  const {
    data: members,
    isPending: areMembersLoading,
    isError: didMembersFail,
    error: membersError,
  } = useQuery({
    queryKey: ["board-members", boardId],
    queryFn: () => getBoardMembers(boardId),
    enabled: open,
  });
  const commentMutation = useMutation({
    mutationFn: (body: string) => addCardComment(card.id, body),
    onSuccess: async () => {
      setComment("");
      await queryClient.invalidateQueries({ queryKey });
    },
  });
  const reactionMutation = useMutation({
    mutationFn: (emoji: string) => toggleCardReaction(card.id, emoji),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey });
    },
  });
  const addAttachmentMutation = useMutation({
    mutationFn: () => addCardAttachment(card.id, attachmentLabel.trim(), attachmentUrl.trim()),
    onSuccess: async () => {
      setAttachmentLabel("");
      setAttachmentUrl("");
      await queryClient.invalidateQueries({ queryKey });
    },
  });
  const deleteAttachmentMutation = useMutation({
    mutationFn: (attachmentId: string) => deleteCardAttachment(card.id, attachmentId),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey });
    },
  });
  const assignMutation = useMutation({
    mutationFn: (assigneeId: string | null) => assignCard(card.id, assigneeId),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey }),
        queryClient.invalidateQueries({ queryKey: ["board", boardId] }),
      ]);
    },
  });
  const deleteMutation = useMutation({
    mutationFn: () => deleteCard(card.id),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["board", boardId] });
      handleOpenChange(false);
    },
  });

  function submitComment(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const body = comment.trim();
    if (body) commentMutation.mutate(body);
  }

  function submitAttachment(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (attachmentLabel.trim() && attachmentUrl.trim()) addAttachmentMutation.mutate();
  }

  return (
    <Dialog.Root open={open} onOpenChange={handleOpenChange}>
      <Dialog.Portal>
        <Dialog.Backdrop className="fixed inset-0 z-40 bg-black/60 backdrop-blur-sm" />
        <Dialog.Viewport className="fixed inset-0 z-50 flex items-center justify-center overflow-y-auto p-4">
          <Dialog.Popup className="my-auto max-h-[90vh] w-full max-w-xl overflow-y-auto rounded-2xl border border-white/15 bg-slate-900 p-6 text-slate-100 shadow-2xl">
            <div className="flex items-start justify-between gap-4">
              <div>
                <Dialog.Title className="text-lg font-semibold">Ticket details</Dialog.Title>
                <Dialog.Description className="mt-1 text-sm text-slate-400">Edit, assign, and discuss this ticket.</Dialog.Description>
              </div>
              <Dialog.Close aria-label="Close ticket details" className="rounded-md p-1 text-slate-400 hover:text-white"><X size={18} /></Dialog.Close>
            </div>

            {isPending && <p className="mt-6 text-sm text-slate-400">Loading ticket...</p>}
            {isError && <p role="alert" className="mt-6 text-sm text-red-300">{error.message}</p>}
            {open && data && (
              <div className="mt-6 space-y-6">
                <section>
                  <h3 className="text-xs font-semibold uppercase tracking-wider text-slate-400">Title and description</h3>
                  <UpdateCardForm boardId={boardId} card={card} />
                </section>
                <section>
                  <h3 className="text-xs font-semibold uppercase tracking-wider text-slate-400">Assignee</h3>
                  <select
                    aria-label="Ticket assignee"
                    value={data.assigneeId ?? ""}
                    onChange={(event) => assignMutation.mutate(event.target.value || null)}
                    disabled={!members || assignMutation.isPending}
                    className="mt-2 w-full rounded-lg border border-white/15 bg-slate-950/60 px-3 py-2 text-sm text-slate-100"
                  >
                    <option value="">Unassigned</option>
                    {members?.map((member) => <option key={member.id} value={member.id}>{member.displayName}{member.isOwner ? " (owner)" : ""}</option>)}
                  </select>
                  {areMembersLoading && <p className="mt-2 text-xs text-slate-400">Loading board members...</p>}
                  {didMembersFail && <p role="alert" className="mt-2 text-sm text-red-300">Could not load assignees: {membersError.message}</p>}
                  {members?.length === 1 && <p className="mt-2 text-xs text-slate-400">Only the board owner is available. To assign someone else, add their registered account through Members at the top of the board.</p>}
                  {assignMutation.isError && <p role="alert" className="mt-2 text-sm text-red-300">{assignMutation.error.message}</p>}
                </section>
                <section>
                  <h3 className="text-xs font-semibold uppercase tracking-wider text-slate-400">Comments</h3>
                  {data.comments.length === 0 && <p className="mt-2 text-sm text-slate-400">No comments yet.</p>}
                  <ul className="mt-3 max-h-56 space-y-3 overflow-y-auto">
                    {data.comments.map((item) => (
                      <li key={item.id} className="rounded-lg border border-white/10 bg-white/5 p-3">
                        <p className="text-xs text-slate-400">{item.authorName} · {new Date(item.createdAt).toLocaleString()}</p>
                        <p className="mt-1 whitespace-pre-wrap break-words text-sm text-slate-200">{item.body}</p>
                      </li>
                    ))}
                  </ul>
                  <form onSubmit={submitComment} className="mt-4 space-y-2">
                    <label htmlFor={`comment-${card.id}`} className="block text-sm font-medium">Add a comment</label>
                    <textarea
                      id={`comment-${card.id}`}
                      value={comment}
                      onChange={(event) => setComment(event.target.value)}
                      maxLength={2000}
                      rows={3}
                      disabled={commentMutation.isPending}
                      className="w-full rounded-lg border border-white/15 bg-slate-950/60 p-3 text-sm outline-none focus:border-cyan-300/60"
                    />
                    {commentMutation.isError && <p role="alert" className="text-sm text-red-300">{commentMutation.error.message}</p>}
                    <button type="submit" disabled={!comment.trim() || commentMutation.isPending} className="rounded-lg bg-cyan-300 px-4 py-2 text-sm font-semibold text-slate-950 disabled:opacity-50">
                      {commentMutation.isPending ? "Posting..." : "Post comment"}
                    </button>
                  </form>
                </section>
                <section>
                  <h3 className="flex items-center gap-2 text-xs font-semibold uppercase tracking-wider text-slate-400"><Paperclip size={14} /> Attachments</h3>
                  {data.attachments.length === 0 && <p className="mt-2 text-sm text-slate-400">No links attached.</p>}
                  <ul className="mt-2 space-y-2">
                    {data.attachments.map((attachment) => (
                      <li key={attachment.id} className="flex items-center justify-between gap-2 rounded-lg border border-white/10 bg-white/5 px-3 py-2 text-sm">
                        <a href={attachment.url} target="_blank" rel="noopener noreferrer" className="inline-flex min-w-0 items-center gap-1 truncate text-cyan-200 hover:underline">
                          {attachment.label} <ExternalLink size={13} className="shrink-0" />
                        </a>
                        <button type="button" aria-label={`Remove ${attachment.label}`} disabled={deleteAttachmentMutation.isPending} onClick={() => deleteAttachmentMutation.mutate(attachment.id)} className="rounded p-1 text-slate-400 hover:text-red-300"><Trash2 size={15} /></button>
                      </li>
                    ))}
                  </ul>
                  <form onSubmit={submitAttachment} className="mt-3 grid gap-2 sm:grid-cols-[1fr_1fr_auto]">
                    <input aria-label="Attachment label" placeholder="Label" value={attachmentLabel} onChange={(event) => setAttachmentLabel(event.target.value)} maxLength={120} required className="min-w-0 rounded-lg border border-white/15 bg-slate-950/60 px-3 py-2 text-sm outline-none focus:border-cyan-300/60" />
                    <input aria-label="Attachment URL" placeholder="https://..." type="url" value={attachmentUrl} onChange={(event) => setAttachmentUrl(event.target.value)} maxLength={2048} required className="min-w-0 rounded-lg border border-white/15 bg-slate-950/60 px-3 py-2 text-sm outline-none focus:border-cyan-300/60" />
                    <button type="submit" disabled={addAttachmentMutation.isPending} className="rounded-lg border border-cyan-300/40 px-3 py-2 text-sm text-cyan-200 disabled:opacity-50">Add link</button>
                  </form>
                  {(addAttachmentMutation.isError || deleteAttachmentMutation.isError) && <p role="alert" className="mt-2 text-sm text-red-300">{addAttachmentMutation.error?.message || deleteAttachmentMutation.error?.message}</p>}
                </section>
                <section>
                  <h3 className="text-xs font-semibold uppercase tracking-wider text-slate-400">Reactions</h3>
                  <div className="mt-2 flex flex-wrap gap-2">
                    {reactions.map((emoji) => {
                      const reaction = data.reactions.find((item) => item.emoji === emoji);
                      return (
                        <button
                          key={emoji}
                          type="button"
                          aria-label={`React with ${emoji}`}
                          aria-pressed={reaction?.reactedByMe ?? false}
                          disabled={reactionMutation.isPending}
                          onClick={() => reactionMutation.mutate(emoji)}
                          className={`rounded-lg border px-3 py-1.5 text-sm ${reaction?.reactedByMe ? "border-cyan-300/60 bg-cyan-300/10" : "border-white/15 hover:bg-white/10"}`}
                        >
                          {emoji} {reaction?.count ?? 0}
                        </button>
                      );
                    })}
                  </div>
                  {reactionMutation.isError && <p role="alert" className="mt-2 text-sm text-red-300">{reactionMutation.error.message}</p>}
                </section>
                <section className="border-t border-white/10 pt-5">
                  {!confirmDelete ? (
                    <button type="button" onClick={() => setConfirmDelete(true)} className="inline-flex items-center gap-2 rounded-lg px-3 py-2 text-sm text-red-300 hover:bg-red-400/10"><Trash2 size={16} /> Delete ticket</button>
                  ) : (
                    <div className="space-y-3">
                      <p className="text-sm text-red-200">Delete this ticket permanently?</p>
                      <div className="flex gap-2">
                        <button type="button" onClick={() => setConfirmDelete(false)} className="rounded-lg border border-white/15 px-3 py-2 text-sm">Cancel</button>
                        <button type="button" disabled={deleteMutation.isPending} onClick={() => deleteMutation.mutate()} className="rounded-lg bg-red-500 px-3 py-2 text-sm font-semibold text-white disabled:opacity-50">{deleteMutation.isPending ? "Deleting..." : "Confirm delete"}</button>
                      </div>
                      {deleteMutation.isError && <p role="alert" className="text-sm text-red-300">{deleteMutation.error.message}</p>}
                    </div>
                  )}
                </section>
              </div>
            )}
          </Dialog.Popup>
        </Dialog.Viewport>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
