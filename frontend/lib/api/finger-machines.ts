import axios from "axios";

import type {
  FingerMachineFormData,
} from "@/types/finger-machine";

const API_URL =
  process.env.NEXT_PUBLIC_API_URL;

export async function createFingerMachine(
  machine: FingerMachineFormData,
) {
  const response = await axios.post(
    `${API_URL}/finger-machines`,
    machine,
  );

  return response.data;
}

export async function updateFingerMachine(
  code: string,
  machine: FingerMachineFormData,
) {
  const response = await axios.put(
    `${API_URL}/finger-machines/${encodeURIComponent(code)}`,
    machine,
  );

  return response.data;
}

export async function deleteFingerMachine(
  code: string,
) {
  const response = await axios.delete(
    `${API_URL}/finger-machines/${encodeURIComponent(code)}`,
  );

  return response.data;
}

export async function getFingerMachinePolicy(
  code: string,
) {
  const response = await axios.get(
    `${API_URL}/finger-machines/${encodeURIComponent(code)}/policy`,
  );

  return response.data;
}

export async function updateFingerMachinePolicy(
  code: string,
  policy: {
    collectionIntervalMinutes: number;
    collectionEnabled: boolean;
    timeSyncIntervalMinutes: number;
    timeSyncEnabled: boolean;
  },
) {
  const response = await axios.put(
    `${API_URL}/finger-machines/${encodeURIComponent(code)}/policy`,
    policy,
  );

  return response.data;
}

export interface FingerMachineGlobalPolicy {
  clearEnabled: boolean;
  clearTime: string;
}

export async function getFingerMachineGlobalPolicy() {
  const response = await axios.get(
    `${API_URL}/finger-machines/global-policy`,
  );

  return response.data;
}

export async function updateFingerMachineGlobalPolicy(
  policy: FingerMachineGlobalPolicy,
) {
  const response = await axios.put(
    `${API_URL}/finger-machines/global-policy`,
    policy,
  );

  return response.data;
}

export interface FingerMachineRuntimeStatus {
  machineCode: string;
  machineName: string;
  isOnline: boolean;
  lastSeenAt: string | null;
  lastPullAt: string | null;
  lastTimeSyncAt: string | null;
  lastClearAt: string | null;
  lastError: string | null;
}

export async function getFingerMachineRuntimeStatus() {
  const response = await axios.get(
    `${API_URL}/finger-machines/runtime/status`,
  );

  return response.data;
}



export interface FingerMachinePullScheduleResponse {
  id: string;
  pullTime: string;
  isActive: boolean;
}


export async function getFingerMachinePullSchedule() {
  const response = await axios.get(
    `${API_URL}/finger-machines/pull-schedule`,
  );

  return response.data;
}


export async function updateFingerMachinePullSchedule(
  pullTimes: string[],
) {
  const response = await axios.put(
    `${API_URL}/finger-machines/pull-schedule`,
    {
      pullTimes,
    },
  );

  return response.data;
}
