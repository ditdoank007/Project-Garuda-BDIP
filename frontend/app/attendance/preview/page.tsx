import AppShell from "@/components/layout/AppShell";
import AttendancePreviewClient from "@/components/attendance/AttendancePreviewClient";
import { getFingerMachines } from "@/services/finger-machine.service";
import type { FingerMachine } from "@/types/finger-machine";

export default async function AttendancePreviewPage() {
  const response = await getFingerMachines();
  const machines: FingerMachine[] = response.data;

  return (
    <AppShell>
      <AttendancePreviewClient machines={machines} />
    </AppShell>
  );
}
