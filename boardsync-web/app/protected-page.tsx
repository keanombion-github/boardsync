"use client";

import { useEffect, type ReactNode } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "./auth-provider";
import { AppSidebar } from "./app-sidebar";

export function ProtectedPage({ children }: { children: ReactNode }) {
  const { user, isInitializing } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isInitializing && !user) {
      router.replace("/login");
    }
  }, [isInitializing, router, user]);

  if (isInitializing || !user) {
    return (
      <main className="grid min-h-screen place-items-center bg-slate-950 text-slate-300">
        <p className="text-sm">Restoring your session...</p>
      </main>
    );
  }

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100 lg:flex">
      <AppSidebar />
      <div className="min-w-0 flex-1">{children}</div>
    </div>
  );
}
