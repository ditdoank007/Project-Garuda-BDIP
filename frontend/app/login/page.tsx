"use client";

import { FormEvent, useEffect, useState } from "react";
import { Logo } from "@/components/common";

type LoginResponse = {
  success?: boolean;
  message?: string;
  data?: {
    sso_redirect?: string;
    ssoRedirect?: string;
  };
};

function InteractiveTitle() {
  const [phase, setPhase] = useState(0);
  const [pointer, setPointer] = useState({ x: -1000, y: -1000 });

  const firstLine = "Satu identitas.";
  const secondLine = "Satu akses.";

  useEffect(() => {
    const timer = window.setInterval(() => {
      setPhase((value) => (value + 0.16) % 26);
    }, 45);

    return () => window.clearInterval(timer);
  }, []);

  function renderLine(text: string, offset: number, gradient = false) {
    return (
      <span className="block">
        {Array.from(text).map((char, index) => {
          const position = offset + index;
          const distance = Math.abs(position - phase);
          const wave = Math.max(0, 1 - distance / 3.5);

          const pointerDistance = Math.sqrt(
            Math.pow(pointer.x - index * 31, 2) +
              Math.pow(
                pointer.y -
                  (offset === 0 ? 32 : 92),
                2,
              ),
          );

          const pointerInfluence = Math.max(
            0,
            1 - pointerDistance / 170,
          );

          const influence = Math.max(wave, pointerInfluence * 0.95);
          const scale = 1 + influence * 0.13;
          const lift = influence * -5;
          const glow = 8 + influence * 28;

          const textClass = gradient
            ? "bg-gradient-to-r from-cyan-200 via-blue-300 to-indigo-300 bg-clip-text text-transparent"
            : "text-white";

          return (
            <span
              key={`${offset}-${index}`}
              className={`inline-block whitespace-pre transition-[transform,text-shadow,opacity] duration-100 ease-out ${textClass}`}
              style={{
                transform: `translateY(${lift}px) scale(${scale})`,
                textShadow:
                  influence > 0
                    ? `0 0 ${glow}px rgba(103,232,249,${0.18 + influence * 0.7})`
                    : "0 0 0 rgba(103,232,249,0)",
                opacity: 0.9 + influence * 0.1,
              }}
            >
              {char === " " ? "\u00A0" : char}
            </span>
          );
        })}
      </span>
    );
  }

  return (
    <div
      className="relative select-none"
      onMouseMove={(event) => {
        const rect = event.currentTarget.getBoundingClientRect();

        setPointer({
          x: event.clientX - rect.left,
          y: event.clientY - rect.top,
        });
      }}
      onMouseLeave={() => {
        setPointer({ x: -1000, y: -1000 });
      }}
    >
      {renderLine(firstLine, 0)}
      {renderLine(secondLine, 15, true)}

      <div
        className="pointer-events-none absolute -inset-10 rounded-3xl bg-cyan-300/[0.03] blur-2xl"
        style={{
          opacity:
            pointer.x > -500
              ? 0.3 + Math.abs(Math.sin(phase)) * 0.35
              : 0.15,
        }}
      />

      <div
        className="pointer-events-none absolute h-1 w-1 rounded-full bg-cyan-100"
        style={{
          left: pointer.x - 2,
          top: pointer.y - 2,
          opacity: pointer.x > -500 ? 1 : 0,
          boxShadow:
            "0 0 8px 3px rgba(103,232,249,.8), 0 0 22px 7px rgba(59,130,246,.35)",
        }}
      />
    </div>
  );
}

export default function LoginPage() {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (loading) {
      return;
    }

    setError("");

    if (!username.trim() || !password) {
      setError("Username dan password wajib diisi.");
      return;
    }

    setLoading(true);

    try {
      const response = await fetch("/api/auth/login", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        credentials: "include",
        body: JSON.stringify({
          username: username.trim(),
          password,
        }),
      });

      const result = (await response.json().catch(() => null)) as
        | LoginResponse
        | null;

      if (!response.ok || result?.success === false) {
        throw new Error(
          result?.message || "Username atau password tidak benar.",
        );
      }

      const ssoRedirect =
        result?.data?.sso_redirect ?? result?.data?.ssoRedirect;

      if (ssoRedirect) {
        window.location.href = ssoRedirect;
        return;
      }

      window.location.href = "/dashboard";
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Login gagal. Silakan coba lagi.",
      );
      setLoading(false);
    }
  }

  return (
    <main className="relative min-h-svh overflow-hidden bg-[#061326] text-white">
      {/* Ambient background */}
      <div className="pointer-events-none absolute inset-0 overflow-hidden">
        <div className="absolute -left-32 -top-32 h-[32rem] w-[32rem] rounded-full bg-blue-500/20 blur-3xl animate-pulse" />
        <div className="absolute -bottom-48 -right-32 h-[36rem] w-[36rem] rounded-full bg-cyan-400/10 blur-3xl" />
        <div className="absolute left-1/2 top-1/3 h-72 w-72 -translate-x-1/2 rounded-full bg-indigo-500/10 blur-3xl" />

        <div
          className="absolute inset-0 opacity-30"
          style={{
            backgroundImage:
              "linear-gradient(rgba(96,165,250,.08) 1px, transparent 1px), linear-gradient(90deg, rgba(96,165,250,.08) 1px, transparent 1px)",
            backgroundSize: "48px 48px",
            maskImage:
              "linear-gradient(to bottom, transparent, black 20%, black 75%, transparent)",
          }}
        />

        <div className="absolute bottom-0 left-0 right-0 h-[38vh] opacity-70">
          <div className="absolute bottom-0 left-[4%] h-32 w-20 bg-slate-900/90 shadow-[0_0_40px_rgba(59,130,246,.15)]" />
          <div className="absolute bottom-0 left-[11%] h-52 w-28 bg-slate-950/90" />
          <div className="absolute bottom-0 left-[20%] h-40 w-16 bg-slate-900/90" />
          <div className="absolute bottom-0 left-[28%] h-64 w-24 bg-slate-950/90" />
          <div className="absolute bottom-0 left-[39%] h-36 w-20 bg-slate-900/90" />
          <div className="absolute bottom-0 right-[38%] h-56 w-24 bg-slate-950/90" />
          <div className="absolute bottom-0 right-[27%] h-40 w-20 bg-slate-900/90" />
          <div className="absolute bottom-0 right-[17%] h-64 w-28 bg-slate-950/90" />
          <div className="absolute bottom-0 right-[6%] h-44 w-24 bg-slate-900/90" />

          <div className="absolute bottom-0 left-0 right-0 h-px bg-blue-300/30 shadow-[0_0_30px_rgba(96,165,250,.7)]" />
        </div>

        <span className="absolute left-[12%] top-[18%] h-1 w-1 rounded-full bg-cyan-200 shadow-[0_0_12px_4px_rgba(103,232,249,.5)] animate-pulse" />
        <span className="absolute left-[72%] top-[16%] h-1.5 w-1.5 rounded-full bg-blue-200 shadow-[0_0_16px_5px_rgba(96,165,250,.5)] animate-pulse" />
        <span className="absolute left-[84%] top-[38%] h-1 w-1 rounded-full bg-cyan-200 shadow-[0_0_12px_4px_rgba(103,232,249,.5)] animate-pulse" />
        <span className="absolute left-[27%] top-[42%] h-1 w-1 rounded-full bg-blue-100 shadow-[0_0_12px_4px_rgba(96,165,250,.4)] animate-pulse" />
      </div>

      <div className="relative z-10 mx-auto flex min-h-svh w-full max-w-7xl items-center justify-center px-5 py-8 lg:px-10">
        <div className="grid w-full overflow-hidden rounded-[2rem] border border-white/15 bg-white/[0.06] shadow-2xl shadow-black/40 backdrop-blur-xl lg:grid-cols-[1.05fr_.95fr]">
          {/* Brand panel */}
          <section className="relative hidden min-h-[650px] overflow-hidden border-r border-white/10 p-10 lg:flex lg:flex-col lg:justify-between">
            <div className="relative z-10">
              <Logo />

              <div className="mt-24 max-w-xl">
                <div className="mb-5 inline-flex items-center gap-2 rounded-full border border-cyan-300/20 bg-cyan-300/10 px-3 py-1.5 text-xs font-medium text-cyan-100">
                  <span className="h-1.5 w-1.5 rounded-full bg-emerald-400 shadow-[0_0_10px_rgba(52,211,153,.8)]" />
                  Secure Identity Platform
                </div>

                <h1 className="text-5xl font-semibold leading-[1.05] tracking-tight text-white xl:text-6xl">
                  <InteractiveTitle />
                </h1>

                <p className="mt-7 max-w-lg text-base leading-7 text-slate-300">
                  Basarnas Digital Identity Platform menyatukan identitas,
                  autentikasi, dan akses aplikasi dalam satu platform yang
                  aman.
                </p>

                <div className="mt-9 flex flex-wrap gap-2">
                  {["LDAP", "SSO", "Identity Governance"].map((item) => (
                    <span
                      key={item}
                      className="rounded-full border border-white/10 bg-white/5 px-3 py-1.5 text-xs text-slate-300"
                    >
                      {item}
                    </span>
                  ))}
                </div>
              </div>
            </div>

            <div className="relative z-10 flex items-end justify-between gap-6 text-xs text-slate-400">
              <span>© Basarnas Digital Identity Platform</span>
              <span className="flex items-center gap-2">
                <span className="h-1.5 w-1.5 rounded-full bg-emerald-400" />
                System Ready
              </span>
            </div>
          </section>

          {/* Login panel */}
          <section className="flex min-h-[650px] items-center justify-center bg-slate-950/35 p-6 sm:p-10">
            <div className="w-full max-w-md">
              <div className="mb-8 lg:hidden">
                <Logo />
              </div>

              <div className="mb-8">
                <div className="mb-4 flex h-11 w-11 items-center justify-center rounded-xl border border-blue-300/15 bg-blue-400/10 text-blue-200">
                  <svg
                    viewBox="0 0 24 24"
                    className="h-5 w-5"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="1.8"
                  >
                    <path d="M12 3 5 6v5c0 4.7 2.9 8.2 7 10 4.1-1.8 7-5.3 7-10V6l-7-3Z" />
                    <path d="m9.5 12 1.7 1.7 3.6-3.7" />
                  </svg>
                </div>

                <h2 className="text-3xl font-semibold tracking-tight text-white">
                  Selamat Datang
                </h2>
                <p className="mt-2 text-sm leading-6 text-slate-400">
                  Masuk untuk mengakses Basarnas Digital Identity Platform.
                </p>
              </div>

              <form onSubmit={handleSubmit} className="space-y-5">
                <div>
                  <label
                    htmlFor="username"
                    className="mb-2 block text-sm font-medium text-slate-300"
                  >
                    Username
                  </label>

                  <div className="group relative">
                    <span className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-slate-500 transition-colors group-focus-within:text-blue-300">
                      <svg
                        viewBox="0 0 24 24"
                        className="h-5 w-5"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="1.8"
                      >
                        <path d="M20 21a8 8 0 0 0-16 0" />
                        <circle cx="12" cy="7" r="4" />
                      </svg>
                    </span>

                    <input
                      id="username"
                      name="username"
                      type="text"
                      autoComplete="username"
                      value={username}
                      onChange={(event) => setUsername(event.target.value)}
                      placeholder="Masukkan username"
                      disabled={loading}
                      className="h-14 w-full rounded-xl border border-white/10 bg-white/[0.05] pl-12 pr-4 text-sm text-white outline-none transition-all duration-300 placeholder:text-slate-600 hover:border-white/20 focus:border-blue-400/60 focus:bg-white/[0.08] focus:ring-4 focus:ring-blue-500/10 disabled:cursor-not-allowed disabled:opacity-60"
                    />
                  </div>
                </div>

                <div>
                  <label
                    htmlFor="password"
                    className="mb-2 block text-sm font-medium text-slate-300"
                  >
                    Password
                  </label>

                  <div className="group relative">
                    <span className="pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 text-slate-500 transition-colors group-focus-within:text-blue-300">
                      <svg
                        viewBox="0 0 24 24"
                        className="h-5 w-5"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="1.8"
                      >
                        <rect x="5" y="10" width="14" height="11" rx="2" />
                        <path d="M8 10V7a4 4 0 0 1 8 0v3" />
                      </svg>
                    </span>

                    <input
                      id="password"
                      name="password"
                      type={showPassword ? "text" : "password"}
                      autoComplete="current-password"
                      value={password}
                      onChange={(event) => setPassword(event.target.value)}
                      placeholder="Masukkan password"
                      disabled={loading}
                      className="h-14 w-full rounded-xl border border-white/10 bg-white/[0.05] pl-12 pr-12 text-sm text-white outline-none transition-all duration-300 placeholder:text-slate-600 hover:border-white/20 focus:border-blue-400/60 focus:bg-white/[0.08] focus:ring-4 focus:ring-blue-500/10 disabled:cursor-not-allowed disabled:opacity-60"
                    />

                    <button
                      type="button"
                      aria-label={
                        showPassword
                          ? "Sembunyikan password"
                          : "Tampilkan password"
                      }
                      onClick={() => setShowPassword((value) => !value)}
                      disabled={loading}
                      className="absolute right-3 top-1/2 flex h-9 w-9 -translate-y-1/2 items-center justify-center rounded-lg text-slate-500 transition hover:bg-white/10 hover:text-slate-200 disabled:pointer-events-none"
                    >
                      {showPassword ? (
                        <svg
                          viewBox="0 0 24 24"
                          className="h-5 w-5"
                          fill="none"
                          stroke="currentColor"
                          strokeWidth="1.8"
                        >
                          <path d="M3 3l18 18" />
                          <path d="M10.6 10.6a2 2 0 0 0 2.8 2.8" />
                          <path d="M9.9 5.2A11.7 11.7 0 0 0 3 12s3 6 9 6c1.6 0 3-.4 4.2-1" />
                          <path d="M14.1 5.3C19 6.7 21 12 21 12s-.8 1.7-2.4 3.2" />
                        </svg>
                      ) : (
                        <svg
                          viewBox="0 0 24 24"
                          className="h-5 w-5"
                          fill="none"
                          stroke="currentColor"
                          strokeWidth="1.8"
                        >
                          <path d="M2.5 12s3.5-6 9.5-6 9.5 6 9.5 6-3.5 6-9.5 6-9.5-6-9.5-6Z" />
                          <circle cx="12" cy="12" r="2.5" />
                        </svg>
                      )}
                    </button>
                  </div>
                </div>

                {error && (
                  <div className="flex items-start gap-3 rounded-xl border border-red-400/20 bg-red-500/10 px-4 py-3 text-sm text-red-200">
                    <svg
                      viewBox="0 0 24 24"
                      className="mt-0.5 h-5 w-5 shrink-0"
                      fill="none"
                      stroke="currentColor"
                      strokeWidth="1.8"
                    >
                      <circle cx="12" cy="12" r="9" />
                      <path d="M12 8v5" />
                      <path d="M12 16.5h.01" />
                    </svg>
                    <span>{error}</span>
                  </div>
                )}

                <button
                  type="submit"
                  disabled={loading}
                  className="group relative flex h-14 w-full items-center justify-center overflow-hidden rounded-xl bg-gradient-to-r from-blue-600 via-blue-500 to-indigo-500 text-sm font-semibold text-white shadow-lg shadow-blue-900/30 transition-all duration-300 hover:-translate-y-0.5 hover:shadow-xl hover:shadow-blue-900/40 focus:outline-none focus:ring-4 focus:ring-blue-500/20 disabled:cursor-not-allowed disabled:opacity-70 disabled:hover:translate-y-0"
                >
                  <span className="absolute inset-0 -translate-x-full bg-gradient-to-r from-transparent via-white/15 to-transparent transition-transform duration-700 group-hover:translate-x-full" />

                  {loading ? (
                    <span className="relative flex items-center gap-3">
                      <span className="h-5 w-5 animate-spin rounded-full border-2 border-white/30 border-t-white" />
                      Memproses login...
                    </span>
                  ) : (
                    <span className="relative flex items-center gap-2">
                      Masuk ke BDIP
                      <svg
                        viewBox="0 0 24 24"
                        className="h-4 w-4 transition-transform duration-300 group-hover:translate-x-1"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2"
                      >
                        <path d="M5 12h14" />
                        <path d="m13 6 6 6-6 6" />
                      </svg>
                    </span>
                  )}
                </button>
              </form>

              <div className="mt-8 flex items-center justify-center gap-2 text-xs text-slate-500">
                <svg
                  viewBox="0 0 24 24"
                  className="h-4 w-4"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="1.8"
                >
                  <path d="M12 3 5 6v5c0 4.7 2.9 8.2 7 10 4.1-1.8 7-5.3 7-10V6l-7-3Z" />
                  <path d="M9.5 12 11 13.5l3.5-3.5" />
                </svg>
                Secure access · Basarnas Digital Identity Platform
              </div>
            </div>
          </section>
        </div>
      </div>
    </main>
  );
}
