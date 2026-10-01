import type { AuthSessionResponse } from "./auth-types";

const API_URL =
  process.env.NEXT_PUBLIC_API_URL
  ?? (process.env.NODE_ENV === "production"
    ? "/backend"
    : "http://localhost:5230");

export type ApiError = {
  code: string;
  message: string;
  details: string[];
};

export type ApiResponse<T> = {
  success: boolean;
  data: T | null;
  error: ApiError | null;
};

export class ApiRequestError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly code?: string,
    public readonly details: string[] = [],
  ) {
    super(message);
    this.name = "ApiRequestError";
  }
}

let accessToken: string | null = null;
let refreshPromise: Promise<AuthSessionResponse | null> | null = null;

export function setAccessToken(token: string | null) {
  accessToken = token;
}

async function readResponse<T>(response: Response): Promise<T> {
  if (response.status === 204) {
    return undefined as T;
  }

  let result: ApiResponse<T> | null = null;

  try {
    result = (await response.json()) as ApiResponse<T>;
  } catch {
    throw new ApiRequestError(
      "The server returned an invalid response.",
      response.status,
    );
  }

  if (!response.ok || !result.success || result.data === null) {
    throw new ApiRequestError(
      result.error?.message ?? "The request failed.",
      response.status,
      result.error?.code,
      result.error?.details,
    );
  }

  return result.data;
}

export async function refreshAccessToken(): Promise<AuthSessionResponse | null> {
  if (refreshPromise) {
    return refreshPromise;
  }

  refreshPromise = (async () => {
    const response = await fetch(`${API_URL}/api/auth/refresh`, {
      method: "POST",
      credentials: "include",
    });

    if (!response.ok) {
      setAccessToken(null);
      return null;
    }

    const session = await readResponse<AuthSessionResponse>(response);
    setAccessToken(session.accessToken);
    return session;
  })().finally(() => {
    refreshPromise = null;
  });

  return refreshPromise;
}

type ApiRequestOptions = {
  retryUnauthorized?: boolean;
  includeAccessToken?: boolean;
};

export async function apiRequest<T>(
  path: string,
  init: RequestInit = {},
  options: ApiRequestOptions = {},
): Promise<T> {
  const {
    retryUnauthorized = true,
    includeAccessToken = true,
  } = options;

  const headers = new Headers(init.headers);

  if (init.body && !(init.body instanceof FormData)) {
    headers.set("Content-Type", "application/json");
  }

  if (includeAccessToken && accessToken) {
    headers.set("Authorization", `Bearer ${accessToken}`);
  }

  const response = await fetch(`${API_URL}${path}`, {
    ...init,
    headers,
    credentials: "include",
  });

  if (response.status === 401 && retryUnauthorized) {
    const session = await refreshAccessToken();

    if (session) {
      return apiRequest<T>(path, init, {
        retryUnauthorized: false,
        includeAccessToken,
      });
    }
  }

  return readResponse<T>(response);
}
