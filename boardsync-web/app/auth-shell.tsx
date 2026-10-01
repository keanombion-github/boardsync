import Link from "next/link";
import type { ReactNode } from "react";

type AuthShellProps = {
  title: string;
  description: string;
  alternateText: string;
  alternateHref: string;
  alternateLabel: string;
  children: ReactNode;
};

export function AuthShell({
  title,
  description,
  alternateText,
  alternateHref,
  alternateLabel,
  children,
}: AuthShellProps) {
  return (
    <main className="relative grid min-h-screen place-items-center overflow-hidden bg-slate-950 px-6 py-12 text-slate-100">
      <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_left,rgba(34,211,238,0.16),transparent_35%),radial-gradient(circle_at_bottom_right,rgba(139,92,246,0.18),transparent_35%)]" />
      <section className="relative w-full max-w-md rounded-3xl border border-white/10 bg-white/7 p-8 shadow-2xl shadow-black/30 backdrop-blur-2xl">
        <p className="text-sm font-semibold tracking-wide text-cyan-300">
          BOARDSYNC
        </p>
        <h1 className="mt-4 text-3xl font-semibold tracking-tight">{title}</h1>
        <p className="mt-2 text-sm leading-6 text-slate-400">{description}</p>

        {children}

        <p className="mt-6 text-center text-sm text-slate-400">
          {alternateText}{" "}
          <Link
            href={alternateHref}
            className="font-medium text-cyan-300 hover:text-cyan-200"
          >
            {alternateLabel}
          </Link>
        </p>
      </section>
    </main>
  );
}
