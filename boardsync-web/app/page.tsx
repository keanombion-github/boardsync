"use client";

import { useQuery } from "@tanstack/react-query";
import { getBoards } from "@/lib/api/boards";

// ⚠️ Hardcoded for now — we'll get this from JWT auth later
const TEST_OWNER_ID = "PASTE-YOUR-USER-UUID-HERE";

export default function HomePage() {
  const { data: boards, isLoading, isError } = useQuery({
    queryKey: ["boards", TEST_OWNER_ID],
    queryFn: () => getBoards(TEST_OWNER_ID),
  });

  if (isLoading) return <p>Loading boards...</p>;
  if (isError) return <p>Failed to load boards.</p>;

  return (
    <main className="p-8">
      <h1 className="text-2xl font-bold mb-4">My Boards</h1>
      <ul className="space-y-2">
        {boards?.map((board) => (
          <li key={board.id} className="p-4 border rounded">
            {board.name}
          </li>
        ))}
      </ul>
    </main>
  );
}
