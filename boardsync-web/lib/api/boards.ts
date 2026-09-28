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

export type CreateBoardInput = {
  name: string;
  ownerId: string;
};

type CreateBoardResult = {
  id: string;
};

type CreateCardResult = {
  id: string;
};

type UpdateCardResult = {
  id: string;
};

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
  const response = await fetch(
    `${API_URL}/api/boards?ownerId=${encodeURIComponent(ownerId)}`,
  );

  const result: ApiResponse<Board[]> = await response.json();

  if (!response.ok || !result.success || !result.data) {
    throw new Error(
      result.error?.message ?? "Failed to fetch boards.",
    );
  }

  return result.data;
}

export async function createBoard(
  input: CreateBoardInput,
): Promise<CreateBoardResult> {
  const response = await fetch(`${API_URL}/api/boards`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      name: input.name,
      ownerId: input.ownerId,
    }),
  });

  const result: ApiResponse<CreateBoardResult> = await response.json();

  if (!response.ok || !result.success || !result.data) {
    throw new Error(
      result.error?.message ?? "Failed to create board.",
    );
  }

  return result.data;
}

export async function getBoardById(boardId: string): Promise<BoardDetail> {
  const response = await fetch(`${API_URL}/api/boards/${boardId}`);
  const result: ApiResponse<BoardDetail> = await response.json();

  if (!response.ok || !result.success || !result.data) {
    throw new Error(
      result.error?.message ?? "Failed to fetch board.",
    );
  }

  return result.data;
}

export async function createCard(
  input: CreateCardInput,
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
      result.error?.message ?? "Failed to create card.",
    );
  }

  return result.data;
}

export async function updateCard(
  input: UpdateCardInput,
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
      result.error?.message ?? "Failed to update card.",
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
      result.error?.message ?? "Failed to delete card.",
    );
  }
}

// Card ordering

export type MoveCardInput = {
  id: string;
  columnId: string;
  beforeCardId: string | null;
  afterCardId: string | null;
};

type MoveCardResult = {
  id: string;
};

export async function moveCard(
  input: MoveCardInput,
): Promise<MoveCardResult> {
  const response = await fetch(`${API_URL}/api/cards/${input.id}/move`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      columnId: input.columnId,
      beforeCardId: input.beforeCardId,
      afterCardId: input.afterCardId,
    }),
  });

  const result: ApiResponse<MoveCardResult> = await response.json();

  if (!response.ok || !result.success || !result.data) {
    throw new Error(
      result.error?.message ?? "Failed to move card.",
    );
  }

  return result.data;
}

// Columns

export type CreateColumnInput = {
  boardId: string;
  name: string;
};

type CreateColumnResult = {
  id: string;
};

export async function createColumn(
  input: CreateColumnInput,
): Promise<CreateColumnResult> {
  const response = await fetch(
    `${API_URL}/api/boards/${input.boardId}/columns`,
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        name: input.name,
      }),
    },
  );

  const result: ApiResponse<CreateColumnResult> = await response.json();

  if (!response.ok || !result.success || !result.data) {
    throw new Error(
      result.error?.message ?? "Failed to create column.",
    );
  }

  return result.data;
}

export async function deleteColumn(
  boardId: string,
  columnId: string,
): Promise<void> {
  const response = await fetch(
    `${API_URL}/api/boards/${boardId}/columns/${columnId}`,
    {
      method: "DELETE",
    },
  );

  if (!response.ok) {
    const result: ApiResponse<never> = await response.json();

    throw new Error(
      result.error?.message ?? "Failed to delete column.",
    );
  }
}

export type ReorderColumnInput = {
  boardId: string;
  columnId: string;
  beforeColumnId: string | null;
  afterColumnId: string | null;
};

type ReorderColumnResult = {
  boardId: string;
};

export async function reorderColumn(
  input: ReorderColumnInput,
): Promise<ReorderColumnResult> {
  const response = await fetch(
    `${API_URL}/api/boards/${input.boardId}/columns/reorder`,
    {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        columnId: input.columnId,
        beforeColumnId: input.beforeColumnId,
        afterColumnId: input.afterColumnId,
      }),
    },
  );

  const result: ApiResponse<ReorderColumnResult> = await response.json();

  if (!response.ok || !result.success || !result.data) {
    throw new Error(
      result.error?.message ?? "Failed to reorder column.",
    );
  }

  return result.data;
}
