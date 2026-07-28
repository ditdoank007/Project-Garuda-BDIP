import AppShell from "@/components/layout/AppShell";
import UnitsClient from "@/components/units/UnitsClient";

import { getUnits } from "@/services/unit.service";

export const dynamic = "force-dynamic";

export default async function UnitsPage() {
  const units = await getUnits();

  return (
    <AppShell>
      <UnitsClient initialUnits={units} />
    </AppShell>
  );
}
