"use client";

import { useEffect, useState, type FormEvent } from "react";
import { useRouter } from "next/navigation";
import { AuthShell } from "../auth-shell";
import { useAuth } from "../auth-provider";

export default function RegisterPage() {
  const [displayName, setDisplayName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { register, user, isInitializing } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isInitializing && user) {
      router.replace("/");
    }
  }, [isInitializing, router, user]);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      await register({ displayName, email, password });
      router.replace("/");
    } catch (submitError) {
      setError(
        submitError instanceof Error
          ? submitError.message
          : "Unable to create your account.",
      );
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <AuthShell
      title="Create your account"
      description="Start with a private workspace. Team collaboration can be added on top of this ownership boundary."
      alternateText="Already have an account?"
      alternateHref="/login"
      alternateLabel="Sign in"
    >
      <form onSubmit={handleSubmit} className="mt-8 space-y-4">
        <InputField
          id="displayName"
          label="Display name"
          value={displayName}
          onChange={setDisplayName}
          autoComplete="name"
        />
        <InputField
          id="email"
          label="Email"
          value={email}
          onChange={setEmail}
          type="email"
          autoComplete="email"
        />
        <InputField
          id="password"
          label="Password"
          value={password}
          onChange={setPassword}
          type="password"
          autoComplete="new-password"
          minLength={8}
          maxLength={128}
        />

        {error && <p className="text-sm text-red-300">{error}</p>}

        <button
          type="submit"
          disabled={isSubmitting || !displayName || !email || !password}
          className="w-full rounded-xl bg-cyan-300 px-4 py-3 text-sm font-semibold text-slate-950 transition hover:bg-cyan-200 disabled:cursor-not-allowed disabled:opacity-50"
        >
          {isSubmitting ? "Creating account..." : "Create account"}
        </button>
      </form>
    </AuthShell>
  );
}

type InputFieldProps = {
  id: string;
  label: string;
  value: string;
  onChange: (value: string) => void;
  type?: "text" | "email" | "password";
  autoComplete: string;
  minLength?: number;
  maxLength?: number;
};

function InputField({
  id,
  label,
  value,
  onChange,
  type = "text",
  autoComplete,
  minLength,
  maxLength,
}: InputFieldProps) {
  return (
    <label htmlFor={id} className="block text-sm font-medium text-slate-300">
      {label}
      <input
        id={id}
        type={type}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        autoComplete={autoComplete}
        minLength={minLength}
        maxLength={maxLength}
        required
        className="mt-2 w-full rounded-xl border border-white/10 bg-slate-950/60 px-4 py-3 text-slate-100 outline-none transition focus:border-cyan-300/60 focus:ring-2 focus:ring-cyan-300/10"
      />
    </label>
  );
}
