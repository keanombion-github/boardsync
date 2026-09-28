import type { BoardColumn as BoardColumnData } from "@/lib/api/boards";
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
};

export default function BoardColumn({ column, boardId }: BoardColumnProps) {
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

  return (
    <section
      ref={setNodeRef}
      style={style}
      className={`w-72 shrink-0 rounded-lg p-4 transition-colors ${
        isOver ? "bg-blue-100" : "bg-gray-100"
      } ${isDragging ? "opacity-60" : ""}`}
    >
      <div className="mb-4 flex items-center justify-between">
        <div className="flex min-w-0 items-center gap-2">
          <button
            type="button"
            aria-label={`Move ${column.name} column`}
            className="cursor-grab rounded p-1 text-gray-500 active:cursor-grabbing"
            {...attributes}
            {...listeners}
          >
            <GripVertical size={16} />
          </button>
          <h2 className="truncate font-semibold">{column.name}</h2>
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
        <div className="min-h-16 space-y-3">
          {column.cards.length === 0 ? (
            <p className="text-sm text-gray-500">No cards yet.</p>
          ) : (
            column.cards.map((card) => (
              <CardItem
                key={card.id}
                boardId={boardId}
                columnId={column.id}
                card={card}
              />
            ))
          )}
        </div>
      </SortableContext>
    </section>
  );
}
