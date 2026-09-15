import AttendanceSyncClient from "@/components/attendance/AttendanceSyncClient";
import { getFingerMachines } from "@/services/finger-machine.service";
import type { FingerMachine } from "@/types/finger-machine";

export default async function AttendanceSynchronizationPage() {
  const response = await getFingerMachines();
  const machines: FingerMachine[] = response.data;

  return (
    <AttendanceSyncClient machines={machines} />
  );
}
