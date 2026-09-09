import AppShell from "@/components/layout/AppShell";
import AttendanceMasterMachineClient from "@/components/attendance/AttendanceMasterMachineClient";
import { getFingerMachines } from "@/services/finger-machine.service";
import type { FingerMachine } from "@/types/finger-machine";

export default async function AttendanceMasterMachinePage() {
  const response = await getFingerMachines();
  const machines: FingerMachine[] = response.data;

  return (
    <AppShell>
      <AttendanceMasterMachineClient machines={machines} />
    </AppShell>
  );
}
