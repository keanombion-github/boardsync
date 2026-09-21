import type { BoardColumn as BoardColumnData } from "@/lib/api/boards";
import { CardItem } from "./card-item";
import { CreateCardDialog } from "./create-card-dialog";

type BoardColumnProps = {
  column: BoardColumnData;
  boardId: string;
};

export default function BoardColumn({ column, boardId }: BoardColumnProps) {
  return (
    <section className="w-72 shrink-0 rounded-lg bg-gray-100 p-4">
      <div className="mb-4 flex items-center justify-between">
        <h2 className="font-semibold">{column.name}</h2>

        <CreateCardDialog
          boardId={boardId}
          columnId={column.id}
        />
      </div>

      <div className="space-y-3">
        {column.cards.length === 0 ? (
          <p className="text-sm text-gray-500">No cards yet.</p>
        ) : (
          column.cards.map((card) => (
            <CardItem
              key={card.id}
              boardId={boardId}
              card={card}
            />
          ))
        )}
      </div>
    </section>
  );
}