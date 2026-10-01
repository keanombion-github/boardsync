import { Fragment } from "react";
import type { BoardColumn as BoardColumnData, Card } from "@/lib/api/boards";
import { CardItem } from "./card-item";
import { CreateCardDialog } from "./create-card-dialog";
import { DeleteColumnDialog } from "./delete-column-dialog";
import {
  SortableContext,
  useSortable,
  verticalListSortingStrategy,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { GripVertical } from "lucide-react";

type BoardColumnProps = {
  column: BoardColumnData;
  boardId: string;
  dropPreview: { card: Card; afterCardId: string | null } | null;
};

export default function BoardColumn({ column, boardId, dropPreview }: BoardColumnProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
    isOver,
  } = useSortable({
    id: column.id,
    data: {
      type: "column",
      columnId: column.id,
    },
  });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  const cardIds = column.cards.map((card) => card.id);
  const previewIndex = dropPreview
    ? dropPreview.afterCardId
      ? column.cards.findIndex((card) => card.id === dropPreview.afterCardId)
      : column.cards.length
    : -1;
  const showPreview = dropPreview !== null && previewIndex >= 0;
  const previewCard = showPreview ? (
    <div className="rounded-xl border-2 border-dashed border-cyan-300/60 bg-cyan-300/10 p-4 text-sm text-cyan-100">
      Drop {dropPreview.card.title} here
    </div>
  ) : null;

  return (
    <section
      ref={setNodeRef}
      style={style}
      className={`w-80 shrink-0 rounded-2xl border p-4 shadow-xl shadow-black/10 backdrop-blur-xl transition ${
        isOver || showPreview
          ? "border-cyan-300/40 bg-cyan-300/10"
          : "border-white/10 bg-white/6"
      } ${isDragging ? "opacity-60" : ""}`}
    >
      <div className="mb-4 flex items-center justify-between">
        <div className="flex min-w-0 items-center gap-2">
          <button
            type="button"
            aria-label={`Move ${column.name} column`}
            className="cursor-grab rounded-md p-1 text-slate-500 hover:bg-white/10 hover:text-slate-200 active:cursor-grabbing"
            {...attributes}
            {...listeners}
          >
            <GripVertical size={16} />
          </button>
          <h2 className="truncate font-semibold text-slate-100">{column.name}</h2>
        </div>

        <div className="flex items-center gap-1">
          <CreateCardDialog boardId={boardId} columnId={column.id} />
          <DeleteColumnDialog boardId={boardId} column={column} />
        </div>
      </div>

      <SortableContext
        items={cardIds}
        strategy={verticalListSortingStrategy}
      >
        <div className="min-h-20 space-y-3">
          {column.cards.length === 0 && !showPreview ? (
            <p className="rounded-xl border border-dashed border-white/10 px-3 py-6 text-center text-sm text-slate-500">
              No cards yet.
            </p>
          ) : (
            column.cards.map((card, index) => (
              <Fragment key={card.id}>
                {index === previewIndex && previewCard}
                <CardItem
                  boardId={boardId}
                  columnId={column.id}
                  card={card}
                />
              </Fragment>
            ))
          )}
          {previewIndex === column.cards.length && previewCard}
        </div>
      </SortableContext>
    </section>
  );
}
