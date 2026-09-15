import { headers } from "next/headers";

import Sidebar from "./Sidebar";
import Header from "./Header";
import BdipAtmosphere from "./BdipAtmosphere";

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
    <BdipAtmosphere>
      <Sidebar />

      <div className="flex min-h-0 min-w-0 flex-1 flex-col overflow-hidden">
        <Header />

        <main className="min-h-0 min-w-0 flex-1 overflow-y-auto overflow-x-hidden bg-transparent p-6">
          {children}
        </main>
      </div>
    </BdipAtmosphere>
  );
}
