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
