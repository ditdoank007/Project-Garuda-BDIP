import AttendanceDiscoveryClient from "@/components/attendance/AttendanceDiscoveryClient";
import { getFingerMachines } from "@/services/finger-machine.service";
import type { FingerMachine } from "@/types/finger-machine";

export default async function AttendanceDiscoveryPage() {
  const response = await getFingerMachines();
  const machines: FingerMachine[] = response.data;

  return (
    <AttendanceDiscoveryClient machines={machines} />
  );
}
