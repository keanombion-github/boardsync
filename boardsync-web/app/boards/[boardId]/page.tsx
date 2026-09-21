import { BoardView } from "./board-view";

type BoardPageProps = {
  params: Promise<{
    boardId: string;
  }>;
};

export default async function BoardPage({
  params,
}: BoardPageProps) {
  const { boardId } = await params;

  return (
    <main className="p-8">
        <BoardView boardId={boardId} />
    </main>
  );
}