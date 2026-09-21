import type { Card } from "@/lib/api/boards";
import { EditCardDialog } from "./edit-card-dialog";
import { DeleteCardDialog } from "./delete-card-dialog";

type CardItemProps = {
  boardId: string;
  card: Card;
};

export function CardItem({ boardId, card }: CardItemProps) {
  return (
    <article className="rounded-md border bg-white p-3 shadow-sm">
      <div className="flex items-start justify-between gap-2">
      <h3>{card.title}</h3>

      <EditCardDialog boardId={boardId} card={card} />
      <DeleteCardDialog boardId={boardId} card={card} />
    </div>

      {card.description && (
        <p className="mt-2 text-sm text-gray-600">
          {card.description}
        </p>
      )}
    </article>
  );
}