"use client";

import { useState } from "react";
import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { LogOut } from "lucide-react";
import { getBoards } from "@/lib/api/boards";
import { CreateBoardDialog } from "./create-board-dialog";
import { ProtectedPage } from "./protected-page";
import { useAuth } from "./auth-provider";
import { EditBoardDialog } from "./edit-board-dialog";
import { DeleteBoardDialog } from "./delete-board-dialog";

export default function HomePage() {
  return (
    <ProtectedPage>
      <Dashboard />
    </ProtectedPage>
  );
}

function Dashboard() {
  const { user, logout } = useAuth();
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const [logoutError, setLogoutError] = useState<string | null>(null);
  const {
    data: boards,
    isLoading,
    isError,
    error,
  } = useQuery({
    queryKey: ["boards"],
    queryFn: getBoards,
  });

  async function handleLogout() {
    setLogoutError(null);
    setIsLoggingOut(true);

    try {
      await logout();
    } catch (error) {
      setLogoutError(
        error instanceof Error ? error.message : "Could not log out. Try again.",
      );
    } finally {
      setIsLoggingOut(false);
    }
  }

  return (
    <main className="min-h-screen bg-slate-950 px-6 py-10 text-slate-100">
      <div className="mx-auto w-full max-w-6xl">
        <header className="mb-10 flex flex-wrap items-center justify-between gap-4">
          <div>
            <p className="text-sm font-medium text-cyan-300">Boardsync</p>
            <h1 className="mt-1 text-3xl font-semibold tracking-tight">
              Boards
            </h1>
            <p className="mt-2 text-sm text-slate-400">
              Signed in as {user?.displayName}
            </p>
          </div>

          <div className="flex items-center gap-3">
            <CreateBoardDialog />
            <button
              type="button"
              onClick={() => void handleLogout()}
              disabled={isLoggingOut}
              className="inline-flex items-center gap-2 rounded-lg border border-white/10 bg-white/5 px-3 py-2 text-sm text-slate-300 hover:bg-white/10"
            >
              <LogOut size={16} />
              {isLoggingOut ? "Logging out..." : "Log out"}
            </button>
          </div>
        </header>

        {logoutError && (
          <p role="alert" className="mb-6 text-sm text-red-300">
            {logoutError}
          </p>
        )}

        {isLoading && <p className="text-slate-400">Loading boards...</p>}

        {isError && (
          <p role="alert" className="text-sm text-red-400">
            {error.message}
          </p>
        )}

        {!isLoading && !isError && boards?.length === 0 && (
          <p className="rounded-2xl border border-dashed border-white/15 bg-white/5 p-10 text-center text-sm text-slate-400 backdrop-blur-xl">
            No boards yet. Create your first workspace to get started.
          </p>
        )}

        {boards && boards.length > 0 && (
          <ul className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {boards.map((board) => (
              <li key={board.id}>
                <article className="rounded-2xl border border-white/10 bg-white/5 p-5 shadow-xl shadow-black/10 backdrop-blur-xl transition hover:-translate-y-0.5 hover:border-cyan-300/30 hover:bg-white/10">
                  <Link href={`/boards/${board.id}`} className="block">
                    <h2 className="font-semibold">{board.name}</h2>
                    <p className="mt-2 text-sm text-slate-400">Open board</p>
                  </Link>
                  <div className="mt-4 flex justify-end gap-2">
                    {board.isOwner && <EditBoardDialog board={board} />}
                    {board.isOwner && <DeleteBoardDialog board={board} />}
                  </div>
                </article>
              </li>
            ))}
          </ul>
        )}
      </div>
    </main>
  );
}
