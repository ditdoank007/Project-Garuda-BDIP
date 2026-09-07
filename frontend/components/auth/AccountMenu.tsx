"use client";

import { LogOut } from "lucide-react";
import { useRouter } from "next/navigation";
import { apiPost } from "@/services/api";

type AuthUser = {
  username: string;
  fullName: string;
  email: string;
  role: string;
};

type AccountMenuProps = {
  user: AuthUser | null;
};

export default function AccountMenu({ user }: AccountMenuProps) {
  const router = useRouter();

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
    <details className="relative group">
      <summary
        className="flex cursor-pointer list-none items-center gap-3 rounded-lg px-2 py-1 text-left transition hover:bg-slate-100"
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
      </summary>

      <div className="absolute right-0 z-50 mt-2 w-56 overflow-hidden rounded-xl border border-slate-200 bg-white py-1 shadow-xl">
        <div className="border-b border-slate-100 px-4 py-3">
          <p className="truncate text-sm font-semibold text-slate-900">
            {fullName}
          </p>
          <p className="text-xs text-slate-500">{role}</p>
        </div>

        <form action="/logout-submit" method="POST">
          <button
            type="submit"
            className="flex w-full items-center gap-2 px-4 py-3 text-left text-sm font-medium text-red-600 transition hover:bg-red-50"
          >
            <LogOut className="h-4 w-4" />
            Logout
          </button>
        </form>
      </div>
    </details>
  );
}
