"use client";

import { FormEvent, useState } from "react";

export default function LoginPage() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (submitting) {
      return;
    }

    setError("");

    const usernameValue = username.trim();

    if (!usernameValue || !password) {
      setError("Username dan password wajib diisi.");
      return;
    }

    try {
      setSubmitting(true);

      const response = await fetch("/api/auth/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({
          username: usernameValue,
          password,
        }),
      });

      const result = await response.json();

      if (!response.ok || !result.success) {
        throw new Error(result.message || "Login gagal.");
      }

      const ssoRedirect =
        new URLSearchParams(window.location.search).get("sso_redirect");

      if (ssoRedirect) {
        window.location.href =
          `/api/auth/sso/start?redirectUri=${encodeURIComponent(ssoRedirect)}`;
        return;
      }

      window.location.href = "/dashboard";
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Login gagal."
      );
      setSubmitting(false);
    }
  }

  return (
    <main className="min-h-screen bg-slate-950 px-4 py-10 text-slate-100">
      <div className="mx-auto flex min-h-[calc(100vh-5rem)] max-w-md items-center">
        <section className="w-full rounded-2xl border border-slate-700 bg-slate-900 p-7 shadow-xl">
          <div className="mb-8 text-center">
            <h1 className="text-2xl font-semibold">
              BDIP
            </h1>

            <p className="mt-2 text-sm text-slate-400">
              Basarnas Digital Identity Platform
            </p>
          </div>

          <form
            className="space-y-5"
            action="/login-submit"
            method="POST"
            onSubmit={handleSubmit}
            noValidate
          >
            <div className="space-y-2">
              <label
                htmlFor="username"
                className="text-sm font-medium"
              >
                Username LDAP
              </label>

              <input
                id="username"
                name="username"
                type="text"
                autoComplete="username"
                value={username}
                onChange={(event) => setUsername(event.target.value)}
                disabled={submitting}
                className="flex h-10 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 outline-none focus:ring-2 focus:ring-slate-500 disabled:opacity-50"
              />
            </div>

            <div className="space-y-2">
              <label
                htmlFor="password"
                className="text-sm font-medium"
              >
                Password
              </label>

              <input
                id="password"
                name="password"
                type="password"
                autoComplete="current-password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                disabled={submitting}
                className="flex h-10 w-full rounded-md border border-slate-700 bg-slate-950 px-3 py-2 text-sm text-slate-100 outline-none focus:ring-2 focus:ring-slate-500 disabled:opacity-50"
              />
            </div>

            {error && (
              <div className="rounded-md border border-red-800 bg-red-950/40 px-3 py-2 text-sm text-red-300">
                {error}
              </div>
            )}

            <button
              className="inline-flex h-10 w-full items-center justify-center rounded-md bg-slate-100 px-4 py-2 text-sm font-medium text-slate-950 transition hover:bg-slate-200 disabled:pointer-events-none disabled:opacity-50"
              type="submit"
              disabled={submitting}
            >
              {submitting ? "Memproses..." : "Masuk ke BDIP"}
            </button>
          </form>
        </section>
      </div>
    </main>
  );
}
