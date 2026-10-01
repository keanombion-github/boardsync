"use client";

import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from "react";
import { useQueryClient } from "@tanstack/react-query";
import {
  login as loginRequest,
  logout as logoutRequest,
  register as registerRequest,
  restoreSession,
  type LoginInput,
  type RegisterInput,
} from "@/lib/api/auth";
import type { AuthUser } from "@/lib/api/auth-types";

type AuthContextValue = {
  user: AuthUser | null;
  isInitializing: boolean;
  login: (input: LoginInput) => Promise<void>;
  register: (input: RegisterInput) => Promise<void>;
  logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [isInitializing, setIsInitializing] = useState(true);
  const queryClient = useQueryClient();

  useEffect(() => {
    let cancelled = false;

    restoreSession()
      .then((session) => {
        if (!cancelled) {
          setUser(session?.user ?? null);
        }
      })
      .catch(() => {
        if (!cancelled) {
          setUser(null);
        }
      })
      .finally(() => {
        if (!cancelled) {
          setIsInitializing(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, []);

  const value = useMemo<AuthContextValue>(() => ({
    user,
    isInitializing,
    async login(input) {
      const session = await loginRequest(input);
      setUser(session.user);
    },
    async register(input) {
      await registerRequest(input);
      const session = await loginRequest({
        email: input.email,
        password: input.password,
      });
      setUser(session.user);
    },
    async logout() {
      await logoutRequest();
      setUser(null);
      queryClient.clear();
    },
  }), [isInitializing, queryClient, user]);

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const value = useContext(AuthContext);

  if (!value) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return value;
}
