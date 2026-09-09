export interface AttendanceMasterMachine {
  machineCode: string;
  machineName: string;
  registered: boolean;
  enabled: boolean;
  deviceUid: number;
  lastSeenAt: string | null;
}

export interface AttendanceMasterUser {
  userId: string;
  isLinked: boolean;
  nip: string;
  fullName: string;
  fingerId: string;
  userEnabled: boolean;
  fingerprintCount: number;
  machines: AttendanceMasterMachine[];
}

export interface AttendanceMasterResponse {
  users: AttendanceMasterUser[];
}

export interface AttendanceDiscoveryUser {
  deviceUid: number;
  fingerId: string;
  deviceName: string;
  bdipFullName: string;
  isMatched: boolean;
  templateCount: number;
}

export interface AttendanceDiscoveryResponse {
  success: boolean;
  machineCode: string;
  machineName: string;
  deviceUserCount: number;
  deviceTemplateCount: number;
  matchedUserCount: number;
  newUserCount: number;
  users: AttendanceDiscoveryUser[];
}

export interface AttendanceDiscoveryImportResponse {
  success: boolean;
  machineCode: string;
  machineName: string;
  importedUserCount: number;
  importedTemplateCount: number;
  skippedAlreadyMatchedCount: number;
}

export interface AttendanceDiscoveryDeleteRequest {
  machineCode: string;
  fingerIds: string[];
}

export interface AttendanceDiscoveryDeleteResult {
  fingerId: string;
  deviceUid: number;
  deviceName: string;
  success: boolean;
  deleted: boolean;
  verified: boolean;
  error: string;
}

export interface AttendanceDiscoveryDeleteResponse {
  success: boolean;
  machineCode: string;
  machineName: string;
  requestedCount: number;
  deletedCount: number;
  failedCount: number;
  skippedCount: number;
  results: AttendanceDiscoveryDeleteResult[];
}
