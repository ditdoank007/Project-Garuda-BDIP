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
    <div className="relative flex h-full min-h-0 overflow-hidden bg-[#061326]">
      <Sidebar />

      <div className="flex min-h-0 min-w-0 flex-1 flex-col overflow-hidden">
        <Header />

        <main
          className={`min-h-0 min-w-0 flex-1 overflow-x-hidden bg-transparent p-6 ${
            pathname === "/audit" ? "overflow-hidden" : "overflow-y-auto"
          }`}
        >
          {children}
        </main>
      </div>
    </div>
  );
}
