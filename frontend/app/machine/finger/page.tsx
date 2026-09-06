import AppShell from "@/components/layout/AppShell";
import FingerMachinesClient from "@/components/machine/FingerMachinesClient";

import {
  getFingerMachines,
} from "@/services/finger-machine.service";

import type {
  FingerMachine,
} from "@/types/finger-machine";

import {
  getLocations,
} from "@/services/location.service";

import type {
  Location,
} from "@/types/location";

export default async function FingerMachinePage() {
  const [
    machineResponse,
    locationResponse,
  ] = await Promise.all([
    getFingerMachines(),
    getLocations(),
  ]);

  const machines: FingerMachine[] =
    machineResponse.data;

  const locations: Location[] =
    locationResponse.data;

  return (
    <AppShell>
      <FingerMachinesClient
        machines={machines}
        locations={locations}
      />
    </AppShell>
  );
}
