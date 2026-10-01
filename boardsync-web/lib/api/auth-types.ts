export type AuthUser = {
  id: string;
  email: string;
  displayName: string;
};

export type AuthSessionResponse = {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  user: AuthUser;
};
