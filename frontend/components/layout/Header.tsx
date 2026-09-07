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
    <header className="flex h-16 items-center justify-between border-b bg-white px-6 shadow-sm">
      <div>
        <h1 className="text-2xl font-bold text-slate-800">
          Dashboard
        </h1>

        <p className="text-sm text-slate-500">
          Welcome to Basarnas Digital Identity Platform
        </p>
      </div>

      <div className="hidden items-center md:flex">
        <div className="flex items-center gap-2 rounded-lg border px-3 py-2">
          <Search size={18} className="text-gray-500" />

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
          className="rounded-md p-1 text-slate-600 transition hover:bg-slate-100"
          aria-label="Notifikasi"
        >
          <Bell size={22} />
        </button>

        <AccountMenu user={user} />
      </div>
    </header>
  );
}
