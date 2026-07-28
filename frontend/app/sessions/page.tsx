import AppShell from "@/components/layout/AppShell";
import SessionsClient from "@/components/sessions/SessionsClient";

import { getSessions } from "@/services/session.service";

export const dynamic = "force-dynamic";

export default async function SessionsPage() {
  const response = await getSessions();

  return (
    <AppShell>
      <SessionsClient initialData={response.data} />
    </AppShell>
  );
}
