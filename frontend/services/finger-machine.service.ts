import { api } from "./api";

import type {
  FingerMachineListResponse,
} from "@/types/finger-machine";

export async function getFingerMachines() {
  return api<FingerMachineListResponse>(
    "/finger-machines",
  );
}
