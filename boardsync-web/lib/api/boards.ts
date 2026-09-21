const API_URL =
  process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5230";

type ApiError = {
  code: string;
  message: string;
  details: string[];
};

type ApiResponse<T> = {
  success: boolean;
  data: T | null;
  error: ApiError | null;
};

type CreateCardResult = {
  id: string;
};

type UpdateCardResult = {
  id: string;
}


export type Board = {
    id: string;
    name: string;
};

export type Card = {
  id: string;
  title: string;
  description: string | null;
  position: number;
};

export type BoardColumn = {
  id: string;
  name: string;
  position: number;
  cards: Card[];
};

export type BoardDetail = {
  id: string;
  name: string;
  columns: BoardColumn[];
};

export type UpdateCardInput = {
  id: string;
  title: string;
  description: string | null;
};


export type CreateCardInput = {
  title: string;
  description: string | null;
  columnId: string;
};


export async function getBoards(ownerId: string): Promise<Board[]> {
    const res = await fetch(`${API_URL}/api/boards?ownerId=${ownerId}`);

    if (!res.ok) throw new Error("Failed to fetch boards");

    const json = await res.json();
    return json.data;
}

export async function getBoardById(
  boardId: string
): Promise<BoardDetail> {
  const response = await fetch(`${API_URL}/api/boards/${boardId}`);

  if (!response.ok) {
    throw new Error("Failed to fetch board.");
  }

 const result: ApiResponse<BoardDetail> = await response.json();

  if (!result.success || !result.data) {
    throw new Error(result.error?.message ?? "Board data is missing.");
  }

  return result.data;
}

export async function createCard(
  input: CreateCardInput
): Promise<CreateCardResult> {
  const response = await fetch(`${API_URL}/api/cards`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(input),
  });

  const result: ApiResponse<CreateCardResult> = await response.json();

  if (!response.ok || !result.success || !result.data) {
    throw new Error(
      result.error?.message ?? "Failed to create card."
    );
  }

  return result.data;
}

export async function updateCard(
  input: UpdateCardInput
): Promise<UpdateCardResult> {
  const response = await fetch(`${API_URL}/api/cards/${input.id}`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      title: input.title,
      description: input.description,
    }),
  });

  const result: ApiResponse<UpdateCardResult> = await response.json();

  if (!response.ok || !result.success || !result.data) {
    throw new Error(
      result.error?.message ?? "Failed to update card."
    );
  }

  return result.data;
}

export async function deleteCard(cardId: string): Promise<void> {
  const response = await fetch(`${API_URL}/api/cards/${cardId}`, {
    method: "DELETE",
  });

  if (!response.ok) {
    const result: ApiResponse<never> = await response.json();

    throw new Error(
      result.error?.message ?? "Failed to delete card."
    );
  }
}


// Move cards 

export type MoveCardInput = {
  id: string;
  columnId: string;
  beforePosition: number | null;
  afterPosition: number | null;
};

type MoveCardResult = {
  id: string;
};

export async function moveCard(
  input: MoveCardInput
): Promise<MoveCardResult> {
  const response = await fetch(`${API_URL}/api/cards/${input.id}/move`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      columnId: input.columnId,
      beforePosition: input.beforePosition,
      afterPosition: input.afterPosition,
    }),
  });

  const result: ApiResponse<MoveCardResult> = await response.json();

  if (!response.ok || !result.success || !result.data) {
    throw new Error(
      result.error?.message ?? "Failed to move card."
    );
  }

  return result.data;
}
