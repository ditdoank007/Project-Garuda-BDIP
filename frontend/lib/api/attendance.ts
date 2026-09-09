import type {
  AttendanceMasterResponse,
  AttendanceDiscoveryResponse,
  AttendanceDiscoveryImportResponse,
  AttendanceDiscoveryDeleteResponse,
} from "@/types/attendance";

export async function getAttendanceMaster(): Promise<AttendanceMasterResponse> {
  const response = await fetch("/api/attendance/master", {
    method: "GET",
    cache: "no-store",
  });

  const data = (await response.json()) as AttendanceMasterResponse & {
    message?: string;
  };

  if (!response.ok) {
    throw new Error(
      data.message || "Gagal mengambil Master Attendance.",
    );
  }

  return data;
}

export async function saveAttendanceMaster(
  fingerId: string,
  fullName: string,
  userId?: string,
): Promise<void> {
  const response = await fetch("/api/attendance/master", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      userId,
      fingerId,
      fullName,
    }),
  });

  const data = (await response.json()) as {
    success?: boolean;
    message?: string;
  };

  if (!response.ok) {
    throw new Error(
      data.message || "Gagal menyimpan Master Attendance.",
    );
  }
}

export async function discoverAttendance(
  machineCode: string,
): Promise<AttendanceDiscoveryResponse> {
  const response = await fetch("/api/attendance/discovery", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      machineCode,
    }),
    cache: "no-store",
  });

  const data = (await response.json()) as AttendanceDiscoveryResponse & {
    message?: string;
  };

  if (!response.ok) {
    throw new Error(
      data.message || "Gagal membaca Discovery dari mesin.",
    );
  }

  return data;
}

export async function importAttendanceDiscovery(
  machineCode: string,
  fingerIds: string[],
): Promise<AttendanceDiscoveryImportResponse> {
  const response = await fetch("/api/attendance/discovery/import", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify({
      machineCode,
      fingerIds,
    }),
  });

  const data =
    (await response.json()) as AttendanceDiscoveryImportResponse & {
      message?: string;
    };

  if (!response.ok) {
    throw new Error(
      data.message || "Gagal meng-import user Discovery.",
    );
  }

  return data;
}

export async function hapusAttendanceDiscovery(
  machineCode: string,
  fingerIds: string[],
): Promise<AttendanceDiscoveryDeleteResponse> {
  const response = await fetch(
    "/api/attendance/discovery/hapus-dari-mesin",
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        machineCode,
        fingerIds,
      }),
    },
  );

  const data =
    (await response.json()) as AttendanceDiscoveryDeleteResponse & {
      message?: string;
    };

  if (!response.ok) {
    throw new Error(
      data.message || "Gagal menghapus user dari mesin.",
    );
  }

  return data;
}

export interface AttendanceMasterMachine {
  machineId: string;
  machineCode: string;
  machineName: string;
}

export async function getAttendanceMasterMachine() {
  const response = await fetch(
    "/api/attendance/master-machine",
    {
      method: "GET",
      cache: "no-store",
    },
  );

  const data = await response.json();

  if (!response.ok) {
    throw new Error(
      data.message ||
        "Gagal mengambil Master Mesin Finger.",
    );
  }

  return data;
}

export async function updateAttendanceMasterMachine(
  machineId: string,
) {
  const response = await fetch(
    "/api/attendance/master-machine",
    {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        machineId,
      }),
    },
  );

  const data = await response.json();

  if (!response.ok) {
    throw new Error(
      data.message ||
        "Gagal menyimpan Master Mesin Finger.",
    );
  }

  return data;
}

export interface AttendanceMachineUserEnabledResponse {
  success: boolean;
  machineCode: string;
  deviceUid: number;
  requestedEnabled: boolean;
  deviceEnabled: boolean;
  databaseUpdated: boolean;
  message: string;
}

export async function setAttendanceMachineUserEnabled(
  machineCode: string,
  deviceUid: number,
  enabled: boolean,
): Promise<AttendanceMachineUserEnabledResponse> {
  const response = await fetch(
    "/api/attendance/machine-user/enabled",
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        machineCode,
        deviceUid,
        enabled,
      }),
    },
  );

  const data =
    (await response.json()) as AttendanceMachineUserEnabledResponse & {
      message?: string;
    };

  if (!response.ok) {
    throw new Error(
      data.message ||
        "Gagal mengubah status user pada Finger Machine.",
    );
  }

  return data;
}

export interface AttendanceSynchronizationMachineResult {
  machineCode: string;
  machineName: string;
  status: "SUCCESS" | "PENDING" | "FAILED";
  matched: number;
  created: number;
  updated: number;
  disabled: number;
  deleted: number;
  templateWritten: number;
  message: string;
}

export interface AttendanceSynchronizationResponse {
  success: boolean;
  startedAt: string;
  finishedAt: string;
  machineCount: number;
  successMachineCount: number;
  pendingMachineCount: number;
  failedMachineCount: number;
  machines: AttendanceSynchronizationMachineResult[];
}

export async function synchronizeAttendanceNow(): Promise<AttendanceSynchronizationResponse> {
  const response = await fetch(
    "/api/attendance/synchronization/sync",
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      cache: "no-store",
      body: JSON.stringify({}),
    },
  );

  const data =
    (await response.json()) as AttendanceSynchronizationResponse & {
      message?: string;
    };

  if (!response.ok) {
    throw new Error(
      data.message ||
        "Gagal menjalankan sinkronisasi Finger Machine.",
    );
  }

  return data;
}

export type AttendanceSynchronizationScheduleFrequency =
  | "DAILY"
  | "WEEKLY"
  | "MONTHLY";

export interface AttendanceSynchronizationSchedule {
  id: string;
  isEnabled: boolean;
  frequency: AttendanceSynchronizationScheduleFrequency;
  syncTime: string;
  weekday: number | null;
  dayOfMonth: number | null;
  lastStartedAt: string | null;
  lastFinishedAt: string | null;
  lastSuccess: boolean | null;
  lastMessage: string | null;
  nextSyncAt: string | null;
}

export interface AttendanceSynchronizationScheduleResponse {
  success: boolean;
  data: AttendanceSynchronizationSchedule;
  message?: string;
}

export async function getAttendanceSynchronizationSchedule(): Promise<AttendanceSynchronizationSchedule> {
  const response = await fetch(
    "/api/attendance/synchronization/schedule",
    {
      method: "GET",
      cache: "no-store",
    },
  );

  const data =
    (await response.json()) as AttendanceSynchronizationScheduleResponse;

  if (!response.ok) {
    throw new Error(
      data.message ||
        "Gagal mengambil konfigurasi jadwal sinkronisasi.",
    );
  }

  return data.data;
}

export async function updateAttendanceSynchronizationSchedule(
  request: {
    isEnabled: boolean;
    frequency: AttendanceSynchronizationScheduleFrequency;
    syncTime: string;
    weekday: number | null;
    dayOfMonth: number | null;
  },
): Promise<AttendanceSynchronizationSchedule> {
  const response = await fetch(
    "/api/attendance/synchronization/schedule",
    {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      cache: "no-store",
      body: JSON.stringify(request),
    },
  );

  const data =
    (await response.json()) as AttendanceSynchronizationScheduleResponse;

  if (!response.ok) {
    throw new Error(
      data.message ||
        "Gagal menyimpan konfigurasi jadwal sinkronisasi.",
    );
  }

  return data.data;
}
