"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { getBoards } from "@/lib/api/boards";
import { CreateBoardDialog } from "./create-board-dialog";

const DEMO_OWNER_ID = process.env.NEXT_PUBLIC_DEMO_OWNER_ID ?? "";

export default function HomePage() {
  const {
    data: boards,
    isLoading,
    isError,
    error,
  } = useQuery({
    queryKey: ["boards", DEMO_OWNER_ID],
    queryFn: () => getBoards(DEMO_OWNER_ID),
    enabled: Boolean(DEMO_OWNER_ID),
  });

  if (!DEMO_OWNER_ID) {
    return (
      <main className="mx-auto w-full max-w-5xl p-8">
        <h1 className="text-2xl font-bold">Boardsync setup</h1>
        <p className="mt-3 max-w-2xl text-sm text-gray-600">
          Set NEXT_PUBLIC_DEMO_OWNER_ID in boardsync-web/.env.local to an
          existing user ID. This temporary development identity will be
          replaced by the authenticated user when JWT authentication is added.
        </p>
      </main>
    );
  }

  return (
    <main className="mx-auto w-full max-w-5xl p-8">
      <div className="mb-6 flex items-center justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold">My boards</h1>
          <p className="text-sm text-gray-600">
            Open a board or create a new workspace.
          </p>
        </div>

        <CreateBoardDialog ownerId={DEMO_OWNER_ID} />
      </div>

      {isLoading && <p>Loading boards...</p>}

      {isError && (
        <p role="alert" className="text-sm text-red-600">
          {error.message}
        </p>
      )}

      {!isLoading && !isError && boards?.length === 0 && (
        <p className="rounded-lg border border-dashed p-8 text-center text-sm text-gray-600">
          No boards yet. Create your first board to get started.
        </p>
      )}

      {boards && boards.length > 0 && (
        <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {boards.map((board) => (
            <li key={board.id}>
              <Link
                href={`/boards/${board.id}`}
                className="block rounded-lg border bg-white p-5 shadow-sm transition hover:-translate-y-0.5 hover:shadow-md"
              >
                <h2 className="font-semibold">{board.name}</h2>
                <p className="mt-2 text-sm text-gray-500">Open board →</p>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </main>
  );
}
