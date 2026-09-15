import { headers } from "next/headers";

import Sidebar from "./Sidebar";
import Header from "./Header";

type AppShellProps = {
  children: React.ReactNode;
};

export default async function AppShell({ children }: AppShellProps) {
  const requestHeaders = await headers();
  const pathname = requestHeaders.get("x-bdip-pathname");

  if (pathname === "/login") {
    return <>{children}</>;
  }

  return (
    <div className="flex h-screen min-h-0 overflow-hidden bg-slate-100">
      <Sidebar />

      <div className="flex min-h-0 min-w-0 flex-1 flex-col overflow-hidden">
        <Header />

        <main className="min-h-0 min-w-0 flex-1 overflow-y-auto overflow-x-hidden p-6">
          {children}
        </main>
      </div>
    </div>
  );
}
