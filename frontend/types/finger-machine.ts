export interface FingerMachine {
  id: string;
  code: string;
  name: string;
  ipAddress: string;
  port: number;
  locationId: string | null;
  locationName: string;
  serialNumber: string;
  deviceName: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface FingerMachineListResponse {
  success: boolean;
  data: FingerMachine[];
}


export interface FingerMachinePolicy {
  machineId: string;
  machineCode: string;
  machineName: string;
  collectionIntervalMinutes: number;
  collectionEnabled: boolean;
  timeSyncIntervalMinutes: number;
  timeSyncEnabled: boolean;
  updatedAt: string;
}

export interface FingerMachinePolicyUpdate {
  collectionIntervalMinutes: number;
  collectionEnabled: boolean;
  timeSyncIntervalMinutes: number;
  timeSyncEnabled: boolean;
}

export interface FingerMachineFormData {
  code: string;
  name: string;
  ipAddress: string;
  port: number;
  locationId: string | null;
  serialNumber: string;
  deviceName: string;
  isActive: boolean;
}
