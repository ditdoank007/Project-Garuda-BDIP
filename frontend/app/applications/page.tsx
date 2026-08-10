import AppShell from "@/components/layout/AppShell";
import ApplicationsClient from "@/components/applications/ApplicationsClient";

import { getApplications } from "@/services/application.service";

export const dynamic = "force-dynamic";

export default async function ApplicationsPage() {
  const response = await getApplications();

  return (
    <AppShell>
      <ApplicationsClient
        applications={response.data}
      />
    </AppShell>
  );
}
