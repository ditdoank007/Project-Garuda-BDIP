import AppShell from "@/components/layout/AppShell";
import LocationsClient from "@/components/locations/LocationsClient";

import { getLocations } from "@/services/location.service";

import type { Location } from "@/types/location";

export default async function LocationsPage() {
  const response = await getLocations();

  const locations: Location[] = response.data;

  return (
    <AppShell>
      <LocationsClient
        locations={locations}
      />
    </AppShell>
  );
}
