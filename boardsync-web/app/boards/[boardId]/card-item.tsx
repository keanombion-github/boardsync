import type { Card } from "@/lib/api/boards";
import { EditCardDialog } from "./edit-card-dialog";
import { DeleteCardDialog } from "./delete-card-dialog";
import { CardActivityDialog } from "./card-activity-dialog";
import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { GripVertical, UserRound } from "lucide-react";

type CardItemProps = {
  boardId: string;
  columnId: string;
  card: Card;
};

export function CardItem({ boardId, columnId, card }: CardItemProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({
    id: card.id,
    data: {
      type: "card",
      cardId: card.id,
      columnId,
    },
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  return (
    <article
      ref={setNodeRef}
      style={style}
      className={`rounded-xl border border-white/10 bg-slate-950/55 p-4 shadow-lg shadow-black/10 transition hover:border-white/20 ${
        isDragging ? "opacity-50" : ""
      }`}
    >
      <div className="flex items-start justify-between gap-2">
        <h3 className="font-medium leading-5 text-slate-100">{card.title}</h3>

        <div className="flex items-center gap-1">
          <EditCardDialog boardId={boardId} card={card} />
          <DeleteCardDialog boardId={boardId} card={card} />
        </div>
      </div>

      {card.description && (
        <p className="mt-2 text-sm leading-5 text-slate-400">
          {card.description}
        </p>
      )}

      <p className="mt-3 inline-flex max-w-full items-center gap-1.5 rounded-full border border-cyan-300/20 bg-cyan-300/10 px-2.5 py-1 text-xs text-cyan-100">
        <UserRound size={13} className="shrink-0" />
        <span className="truncate">{card.assigneeName ?? "Unassigned"}</span>
      </p>

      <div className="mt-3 flex items-center justify-between gap-2">
        <CardActivityDialog boardId={boardId} card={card} />
        <button
          type="button"
          aria-label={`Drag ${card.title}`}
          {...listeners}
          {...attributes}
          className="cursor-grab rounded-md p-1 text-slate-500 hover:bg-white/10 hover:text-slate-300 active:cursor-grabbing"
        >
          <GripVertical size={18} />
        </button>
      </div>
    </article>
  );
}
