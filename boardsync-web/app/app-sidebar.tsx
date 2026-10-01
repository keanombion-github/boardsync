"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { LayoutDashboard, PanelsTopLeft } from "lucide-react";
import { getBoards } from "@/lib/api/boards";

export function AppSidebar() {
  const pathname = usePathname();
  const { data: boards, isPending, isError } = useQuery({
    queryKey: ["boards"],
    queryFn: getBoards,
  });

  return (
    <aside className="border-b border-white/10 bg-slate-900/80 p-4 backdrop-blur-xl lg:sticky lg:top-0 lg:h-screen lg:w-64 lg:shrink-0 lg:overflow-y-auto lg:border-b-0 lg:border-r lg:p-5">
      <Link href="/" className="flex items-center gap-2 text-base font-semibold tracking-tight text-white">
        <span className="rounded-lg bg-cyan-400/15 p-2 text-cyan-300"><PanelsTopLeft size={18} /></span>
        Boardsync
      </Link>
      <nav aria-label="Workspace navigation" className="mt-5">
        <Link
          href="/"
          aria-current={pathname === "/" ? "page" : undefined}
          className={`flex items-center gap-2 rounded-lg px-3 py-2 text-sm transition ${pathname === "/" ? "bg-cyan-400/10 text-cyan-200" : "text-slate-400 hover:bg-white/5 hover:text-white"}`}
        >
          <LayoutDashboard size={17} /> All boards
        </Link>
        <p className="mt-6 px-3 text-xs font-semibold uppercase tracking-wider text-slate-500">Boards</p>
        {isPending && <p className="px-3 py-3 text-sm text-slate-500">Loading...</p>}
        {isError && <p role="alert" className="px-3 py-3 text-sm text-red-300">Could not load boards.</p>}
        {boards?.length === 0 && <p className="px-3 py-3 text-sm text-slate-500">No boards yet.</p>}
        <ul className="mt-2 flex gap-1 overflow-x-auto lg:block">
          {boards?.map((board) => {
            const href = `/boards/${board.id}`;
            const selected = pathname === href;
            return (
              <li key={board.id} className="shrink-0 lg:mb-1">
                <Link
                  href={href}
                  aria-current={selected ? "page" : undefined}
                  title={board.name}
                  className={`block max-w-48 truncate rounded-lg px-3 py-2 text-sm transition lg:max-w-none ${selected ? "bg-cyan-400/10 text-cyan-200" : "text-slate-400 hover:bg-white/5 hover:text-white"}`}
                >
                  {board.name}
                </Link>
              </li>
            );
          })}
        </ul>
      </nav>
    </aside>
  );
}
