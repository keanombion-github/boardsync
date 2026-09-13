const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

export type Board = {
    id: string;
    name: string;
};

export async function getBoards(ownerId: string): Promise<Board[]> {
    const res = await fetch(`${API_URL}/api/boards?ownerId=${ownerId}`);

    if (!res.ok) throw new Error("Failed to fetch boards");

    const json = await res.json();
    return json.data;
}
