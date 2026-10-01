import { BoardView } from "./board-view";
import { ProtectedPage } from "../../protected-page";

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
    <ProtectedPage>
      <main className="relative min-h-screen overflow-hidden bg-slate-950 text-slate-100">
        <div className="pointer-events-none absolute inset-0 bg-[radial-gradient(circle_at_top_left,rgba(34,211,238,0.10),transparent_30%),radial-gradient(circle_at_bottom_right,rgba(139,92,246,0.12),transparent_35%)]" />
        <div className="relative">
        <BoardView boardId={boardId} />
        </div>
      </main>
    </ProtectedPage>
  );
}
