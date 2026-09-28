import type { Card } from "@/lib/api/boards";
import { EditCardDialog } from "./edit-card-dialog";
import { DeleteCardDialog } from "./delete-card-dialog";
import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { GripVertical } from "lucide-react";

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
      className={`rounded-md border bg-white p-3 shadow-sm ${
        isDragging ? "opacity-50" : ""
      }`}
    >
      <div className="flex items-start justify-between gap-2">
        <h3>{card.title}</h3>

        <div className="flex items-center gap-1">
          <EditCardDialog boardId={boardId} card={card} />
          <DeleteCardDialog boardId={boardId} card={card} />
        </div>
      </div>

      {card.description && (
        <p className="mt-2 text-sm text-gray-600">
          {card.description}
        </p>
      )}

      <button
        type="button"
        aria-label={`Drag ${card.title}`}
        {...listeners}
        {...attributes}
      >
        <GripVertical size={18} />
      </button>
    </article>
  );
}
