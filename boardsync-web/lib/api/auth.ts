import {
  apiRequest,
  refreshAccessToken,
  setAccessToken,
} from "./client";
import type { AuthSessionResponse } from "./auth-types";

export type RegisterInput = {
  email: string;
  password: string;
  displayName: string;
};

export type LoginInput = {
  email: string;
  password: string;
};

type RegisterResult = {
  id: string;
  email: string;
  displayName: string;
};

export function register(input: RegisterInput): Promise<RegisterResult> {
  return apiRequest<RegisterResult>(
    "/api/auth/register",
    {
      method: "POST",
      body: JSON.stringify(input),
    },
    {
      retryUnauthorized: false,
      includeAccessToken: false,
    },
  );
}

export async function login(
  input: LoginInput,
): Promise<AuthSessionResponse> {
  const session = await apiRequest<AuthSessionResponse>(
    "/api/auth/login",
    {
      method: "POST",
      body: JSON.stringify(input),
    },
    {
      retryUnauthorized: false,
      includeAccessToken: false,
    },
  );

  setAccessToken(session.accessToken);
  return session;
}

export function restoreSession(): Promise<AuthSessionResponse | null> {
  return refreshAccessToken();
}

export async function logout(): Promise<void> {
  await apiRequest<void>(
    "/api/auth/logout",
    { method: "POST" },
    { retryUnauthorized: false },
  );
  setAccessToken(null);
}
