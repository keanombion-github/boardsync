import { apiRequest } from "./client";

export type CreateBoardInput = {
  name: string;
};

export type UpdateBoardInput = {
  id: string;
  name: string;
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
  isOwner: boolean;
};

export type BoardMember = {
  id: string;
  displayName: string;
  email: string;
  isOwner: boolean;
};

export function getBoardMembers(boardId: string): Promise<BoardMember[]> {
  return apiRequest<BoardMember[]>(`/api/boards/${boardId}/members`);
}

export function addBoardMember(boardId: string, email: string): Promise<{ email: string }> {
  return apiRequest<{ email: string }>(`/api/boards/${boardId}/members`, {
    method: "POST",
    body: JSON.stringify({ email }),
  });
}

export function removeBoardMember(boardId: string, memberId: string): Promise<void> {
  return apiRequest<void>(`/api/boards/${boardId}/members/${memberId}`, {
    method: "DELETE",
  });
}

export function assignCard(cardId: string, assigneeId: string | null): Promise<{ id: string }> {
  return apiRequest<{ id: string }>(`/api/cards/${cardId}/assignee`, {
    method: "PUT",
    body: JSON.stringify({ assigneeId }),
  });
}

export type Card = {
  id: string;
  title: string;
  description: string | null;
  position: number;
  assigneeId: string | null;
  assigneeName: string | null;
};

export type CardActivity = {
  id: string;
  title: string;
  description: string | null;
  assigneeId: string | null;
  assigneeName: string | null;
  comments: {
    id: string;
    body: string;
    authorId: string;
    authorName: string;
    createdAt: string;
  }[];
  reactions: {
    emoji: string;
    count: number;
    reactedByMe: boolean;
  }[];
  attachments: {
    id: string;
    label: string;
    url: string;
    addedBy: string;
    createdAt: string;
  }[];
};

export function getCardActivity(cardId: string): Promise<CardActivity> {
  return apiRequest<CardActivity>(`/api/cards/${cardId}/activity`);
}

export function addCardComment(cardId: string, body: string): Promise<{ id: string }> {
  return apiRequest<{ id: string }>(`/api/cards/${cardId}/comments`, {
    method: "POST",
    body: JSON.stringify({ body }),
  });
}

export function toggleCardReaction(cardId: string, emoji: string): Promise<{ id: string }> {
  return apiRequest<{ id: string }>(`/api/cards/${cardId}/reactions`, {
    method: "PUT",
    body: JSON.stringify({ emoji }),
  });
}

export function addCardAttachment(
  cardId: string,
  label: string,
  url: string,
): Promise<{ id: string }> {
  return apiRequest<{ id: string }>(`/api/cards/${cardId}/attachments`, {
    method: "POST",
    body: JSON.stringify({ label, url }),
  });
}

export function deleteCardAttachment(cardId: string, attachmentId: string): Promise<void> {
  return apiRequest<void>(`/api/cards/${cardId}/attachments/${attachmentId}`, {
    method: "DELETE",
  });
}

export type BoardColumn = {
  id: string;
  name: string;
  position: number;
  cards: Card[];
};

export type BoardDetail = {
  id: string;
  name: string;
  isOwner: boolean;
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

export function getBoards(): Promise<Board[]> {
  return apiRequest<Board[]>("/api/boards");
}

export async function createBoard(
  input: CreateBoardInput,
): Promise<CreateBoardResult> {
  return apiRequest<CreateBoardResult>("/api/boards", {
    method: "POST",
    body: JSON.stringify({
      name: input.name,
    }),
  });
}

export function updateBoard(input: UpdateBoardInput): Promise<Board> {
  return apiRequest<Board>(`/api/boards/${input.id}`, {
    method: "PUT",
    body: JSON.stringify({ name: input.name }),
  });
}

export function deleteBoard(boardId: string): Promise<void> {
  return apiRequest<void>(`/api/boards/${boardId}`, {
    method: "DELETE",
  });
}

export function getBoardById(boardId: string): Promise<BoardDetail> {
  return apiRequest<BoardDetail>(`/api/boards/${boardId}`);
}

export async function createCard(
  input: CreateCardInput,
): Promise<CreateCardResult> {
  return apiRequest<CreateCardResult>("/api/cards", {
    method: "POST",
    body: JSON.stringify(input),
  });
}

export async function updateCard(
  input: UpdateCardInput,
): Promise<UpdateCardResult> {
  return apiRequest<UpdateCardResult>(`/api/cards/${input.id}`, {
    method: "PUT",
    body: JSON.stringify({
      title: input.title,
      description: input.description,
    }),
  });
}

export async function deleteCard(cardId: string): Promise<void> {
  return apiRequest<void>(`/api/cards/${cardId}`, {
    method: "DELETE",
  });
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
  return apiRequest<MoveCardResult>(`/api/cards/${input.id}/move`, {
    method: "PUT",
    body: JSON.stringify({
      columnId: input.columnId,
      beforeCardId: input.beforeCardId,
      afterCardId: input.afterCardId,
    }),
  });
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
  return apiRequest<CreateColumnResult>(
    `/api/boards/${input.boardId}/columns`,
    {
      method: "POST",
      body: JSON.stringify({
        name: input.name,
      }),
    },
  );
}

export async function deleteColumn(
  boardId: string,
  columnId: string,
): Promise<void> {
  return apiRequest<void>(
    `/api/boards/${boardId}/columns/${columnId}`,
    {
      method: "DELETE",
    },
  );
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
  return apiRequest<ReorderColumnResult>(
    `/api/boards/${input.boardId}/columns/reorder`,
    {
      method: "PUT",
      body: JSON.stringify({
        columnId: input.columnId,
        beforeColumnId: input.beforeColumnId,
        afterColumnId: input.afterColumnId,
      }),
    },
  );
}
