"use client";

import { LogOut } from "lucide-react";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";

const API_URL =
  process.env.NEXT_PUBLIC_API_URL ??
  "http://192.168.100.120:8080/api";

type AuthUser = {
  username: string;
  fullName: string;
  email: string;
  role: string;
};

type ApiResponse<T> = {
  success: boolean;
  data?: T;
};

export default function AccountMenu() {
  const router = useRouter();

  const [user, setUser] = useState<AuthUser | null>(null);
  const [open, setOpen] = useState(false);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    async function loadCurrentUser() {
      try {
        const response = await fetch(`${API_URL}/auth/me`, {
          credentials: "include",
          cache: "no-store",
        });

        if (!response.ok) {
          return;
        }

        const result = (await response.json()) as ApiResponse<AuthUser>;

        if (result.success && result.data) {
          setUser(result.data);
        }
      } catch {
        // Middleware tetap menjadi lapisan proteksi halaman.
      }
    }

    loadCurrentUser();
  }, []);

  async function handleLogout() {
    setLoading(true);

    try {
      await fetch(`${API_URL}/auth/logout`, {
        method: "POST",
        credentials: "include",
      });
    } finally {
      router.replace("/login");
      router.refresh();
    }
  }

  const fullName = user?.fullName ?? "Memuat...";
  const role = user?.role ?? "";
  const initials = (user?.fullName ?? "BDIP")
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase();

  return (
    <div className="relative">
      <button
        type="button"
        onClick={() => setOpen((value) => !value)}
        className="flex items-center gap-3 rounded-lg px-2 py-1 text-left transition hover:bg-slate-100"
        aria-expanded={open}
        aria-label="Buka menu akun"
      >
        <span className="flex h-10 w-10 items-center justify-center rounded-full bg-blue-600 text-sm font-semibold text-white">
          {initials}
        </span>

        <span className="hidden min-w-0 sm:block">
          <span className="block max-w-44 truncate text-sm font-semibold text-slate-900">
            {fullName}
          </span>
          <span className="block text-xs text-slate-500">{role}</span>
        </span>
      </button>

      {open && (
        <div className="absolute right-0 z-50 mt-2 w-56 overflow-hidden rounded-xl border border-slate-200 bg-white py-1 shadow-xl">
          <div className="border-b border-slate-100 px-4 py-3">
            <p className="truncate text-sm font-semibold text-slate-900">
              {fullName}
            </p>
            <p className="text-xs text-slate-500">{role}</p>
          </div>

          <button
            type="button"
            onClick={handleLogout}
            disabled={loading}
            className="flex w-full items-center gap-2 px-4 py-3 text-left text-sm font-medium text-red-600 transition hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <LogOut className="h-4 w-4" />
            {loading ? "Keluar..." : "Logout"}
          </button>
        </div>
      )}
    </div>
  );
}
