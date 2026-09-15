import { Bell, Search } from "lucide-react";
import { cookies } from "next/headers";
import AccountMenu from "@/components/auth/AccountMenu";
import { getCurrentUser } from "@/services/auth.service";

export default async function Header() {
  const cookieStore = await cookies();
  const cookieHeader = cookieStore
    .getAll()
    .map((cookie) => `${cookie.name}=${cookie.value}`)
    .join("; ");

  const user = await getCurrentUser(cookieHeader);

  return (
    <header className="sticky top-0 z-40 flex h-16 shrink-0 items-center justify-between border-b border-white/10 bg-slate-950/70 px-6 text-white shadow-xl shadow-black/10 backdrop-blur-xl">
      <div>
        <h1 className="text-2xl font-bold text-white">
          Dashboard
        </h1>

        <p className="text-sm text-slate-400">
          Welcome to Basarnas Digital Identity Platform
        </p>
      </div>

      <div className="hidden items-center md:flex">
        <div className="flex items-center gap-2 rounded-lg border border-white/10 bg-white/[0.05] px-3 py-2">
          <Search size={18} className="text-slate-400" />

          <input
            type="text"
            placeholder="Search..."
            className="w-48 outline-none"
          />
        </div>
      </div>

      <div className="flex items-center gap-6">
        <button
          type="button"
          className="rounded-md p-2 text-slate-400 transition hover:bg-white/10 hover:text-white"
          aria-label="Notifikasi"
        >
          <Bell size={22} />
        </button>

        <AccountMenu user={user} />
      </div>
    </header>
  );
}
