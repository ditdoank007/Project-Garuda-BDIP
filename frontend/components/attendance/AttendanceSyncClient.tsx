"use client";

import { useEffect, useMemo, useState } from "react";

import {
  getAttendanceSynchronizationSchedule,
  synchronizeAttendanceNow,
  updateAttendanceSynchronizationSchedule,
} from "@/lib/api/attendance";
import type { FingerMachine } from "@/types/finger-machine";

type SyncSchedule = "DAILY" | "WEEKLY" | "MONTHLY";

interface AttendanceSyncClientProps {
  machines: FingerMachine[];
}

interface SyncResult {
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

const dayOptions = [
  { value: "MONDAY", label: "Senin" },
  { value: "TUESDAY", label: "Selasa" },
  { value: "WEDNESDAY", label: "Rabu" },
  { value: "THURSDAY", label: "Kamis" },
  { value: "FRIDAY", label: "Jumat" },
  { value: "SATURDAY", label: "Sabtu" },
  { value: "SUNDAY", label: "Minggu" },
];

export default function AttendanceSyncClient({
  machines,
}: AttendanceSyncClientProps) {
  const activeMachines = useMemo(
    () => machines.filter((machine) => machine.isActive),
    [machines],
  );

  const [schedule, setSchedule] =
    useState<SyncSchedule>("DAILY");

  const [scheduleTime, setScheduleTime] =
    useState("02:00");

  const [weeklyDay, setWeeklyDay] =
    useState("MONDAY");

  const [monthlyDay, setMonthlyDay] =
    useState("1");

  const [enabled, setEnabled] = useState(false);

  const [saving, setSaving] = useState(false);
  const [loadingSchedule, setLoadingSchedule] = useState(true);
  const [syncing, setSyncing] = useState(false);

  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const [lastSync, setLastSync] =
    useState<string | null>(null);

  const [nextSyncAt, setNextSyncAt] =
    useState<string | null>(null);

  const [lastScheduledSync, setLastScheduledSync] =
    useState<string | null>(null);

  const [results, setResults] =
    useState<SyncResult[]>([]);

  const scheduleLabel =
    schedule === "DAILY"
      ? `Harian ${scheduleTime}`
      : schedule === "WEEKLY"
        ? `Mingguan ${weeklyDay} ${scheduleTime}`
        : `Bulanan tanggal ${monthlyDay} ${scheduleTime}`;

  useEffect(() => {
    let cancelled = false;

    async function loadSchedule() {
      setLoadingSchedule(true);
      setError("");

      try {
        const data = await getAttendanceSynchronizationSchedule();

        if (cancelled) {
          return;
        }

        setEnabled(data.isEnabled);
        setSchedule(data.frequency);
        setScheduleTime(data.syncTime);
        setNextSyncAt(data.nextSyncAt);
        setLastScheduledSync(data.lastFinishedAt);

        if (data.weekday !== null) {
          setWeeklyDay(dayOptions[data.weekday - 1]?.value ?? "MONDAY");
        }

        if (data.dayOfMonth !== null) {
          setMonthlyDay(String(data.dayOfMonth));
        }
      } catch (err) {
        if (!cancelled) {
          setError(
            err instanceof Error
              ? err.message
              : "Gagal mengambil konfigurasi jadwal sinkronisasi.",
          );
        }
      } finally {
        if (!cancelled) {
          setLoadingSchedule(false);
        }
      }
    }

    void loadSchedule();

    return () => {
      cancelled = true;
    };
  }, []);

  async function saveSchedule() {
    setSaving(true);
    setMessage("");
    setError("");

    try {
      const weekday =
        schedule === "WEEKLY"
          ? dayOptions.findIndex(
              (option) => option.value === weeklyDay,
            ) + 1
          : null;

      const dayOfMonth =
        schedule === "MONTHLY"
          ? Number(monthlyDay)
          : null;

      const data =
        await updateAttendanceSynchronizationSchedule({
          isEnabled: enabled,
          frequency: schedule,
          syncTime: scheduleTime,
          weekday,
          dayOfMonth,
        });

      setEnabled(data.isEnabled);
      setSchedule(data.frequency);
      setScheduleTime(data.syncTime);
      setNextSyncAt(data.nextSyncAt);
      setLastScheduledSync(data.lastFinishedAt);

      if (data.weekday !== null) {
        setWeeklyDay(
          dayOptions[data.weekday - 1]?.value ?? "MONDAY",
        );
      }

      if (data.dayOfMonth !== null) {
        setMonthlyDay(String(data.dayOfMonth));
      }

      setMessage(
        `Konfigurasi jadwal ${scheduleLabel} berhasil disimpan.`,
      );
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Gagal menyimpan jadwal.",
      );
    } finally {
      setSaving(false);
    }
  }

  async function syncNow() {
    if (activeMachines.length === 0) {
      setError("Tidak ada Finger Machine aktif.");
      return;
    }

    const confirmed = window.confirm(
      `Sinkronisasi Master BDIP ke ${activeMachines.length} mesin aktif sekarang?`,
    );

    if (!confirmed) {
      return;
    }

    setSyncing(true);
    setMessage("");
    setError("");
    setResults([]);

    try {
      const result =
        await synchronizeAttendanceNow();

      setLastSync(
        new Date(result.finishedAt).toLocaleString(
          "id-ID",
          {
            dateStyle: "medium",
            timeStyle: "short",
          },
        ),
      );

      setResults(result.machines);

      setMessage(
        `Sinkronisasi selesai: ${result.successMachineCount} mesin berhasil, ` +
          `${result.pendingMachineCount} pending, ` +
          `${result.failedMachineCount} gagal.`,
      );
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Gagal menjalankan sinkronisasi.",
      );
    } finally {
      setSyncing(false);
    }
  }

  return (
    <section className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
      <div className="border-b border-gray-100 pb-5">
        <div className="flex flex-col gap-2 md:flex-row md:items-start md:justify-between">
          <div>
            <h2 className="text-lg font-semibold text-gray-900">
              Sinkronisasi Attendance
            </h2>

            <p className="mt-1 text-sm text-gray-500">
              Sumber data:{" "}
              <span className="font-medium text-gray-700">
                MASTER ATTENDANCE BDIP
              </span>
            </p>

            <p className="text-sm text-gray-500">
              Target:{" "}
              <span className="font-medium text-gray-700">
                Semua Mesin Finger Aktif
              </span>
            </p>
          </div>

          <span
            className={`inline-flex w-fit rounded-full px-3 py-1 text-xs font-semibold ${
              enabled
                ? "bg-green-100 text-green-700"
                : "bg-gray-100 text-gray-600"
            }`}
          >
            {enabled
              ? "SINKRONISASI AKTIF"
              : "SINKRONISASI NONAKTIF"}
          </span>
        </div>
      </div>

      <div className="border-b border-gray-100 py-5">
        <div className="text-sm font-semibold text-gray-900">
          Jadwal Sinkronisasi Otomatis
        </div>

        <label className="mt-4 flex cursor-pointer items-center gap-3">
          <input
            type="checkbox"
            checked={enabled}
            onChange={(event) =>
              setEnabled(event.target.checked)
            }
            className="h-4 w-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
          />

          <span className="text-sm font-medium text-gray-700">
            Aktifkan sinkronisasi otomatis
          </span>
        </label>

        <div className="mt-4 space-y-3">
          <label className="flex flex-wrap items-center gap-3">
            <input
              type="radio"
              name="attendance-schedule"
              checked={schedule === "DAILY"}
              onChange={() => setSchedule("DAILY")}
              className="h-4 w-4 border-gray-300 text-blue-600 focus:ring-blue-500"
            />

            <span className="w-24 text-sm font-medium text-gray-700">
              Harian
            </span>

            <input
              type="time"
              value={scheduleTime}
              onChange={(event) =>
                setScheduleTime(event.target.value)
              }
              disabled={
                !enabled || schedule !== "DAILY"
              }
              className="rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-400"
            />
          </label>

          <label className="flex flex-wrap items-center gap-3">
            <input
              type="radio"
              name="attendance-schedule"
              checked={schedule === "WEEKLY"}
              onChange={() => setSchedule("WEEKLY")}
              className="h-4 w-4 border-gray-300 text-blue-600 focus:ring-blue-500"
            />

            <span className="w-24 text-sm font-medium text-gray-700">
              Mingguan
            </span>

            <select
              value={weeklyDay}
              onChange={(event) =>
                setWeeklyDay(event.target.value)
              }
              disabled={
                !enabled || schedule !== "WEEKLY"
              }
              className="rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-400"
            >
              {dayOptions.map((day) => (
                <option
                  key={day.value}
                  value={day.value}
                >
                  {day.label}
                </option>
              ))}
            </select>

            <input
              type="time"
              value={scheduleTime}
              onChange={(event) =>
                setScheduleTime(event.target.value)
              }
              disabled={
                !enabled || schedule !== "WEEKLY"
              }
              className="rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-400"
            />
          </label>

          <label className="flex flex-wrap items-center gap-3">
            <input
              type="radio"
              name="attendance-schedule"
              checked={schedule === "MONTHLY"}
              onChange={() => setSchedule("MONTHLY")}
              className="h-4 w-4 border-gray-300 text-blue-600 focus:ring-blue-500"
            />

            <span className="w-24 text-sm font-medium text-gray-700">
              Bulanan
            </span>

            <select
              value={monthlyDay}
              onChange={(event) =>
                setMonthlyDay(event.target.value)
              }
              disabled={
                !enabled || schedule !== "MONTHLY"
              }
              className="rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-400"
            >
              {Array.from(
                { length: 31 },
                (_, index) => index + 1,
              ).map((day) => (
                <option
                  key={day}
                  value={String(day)}
                >
                  {day}
                </option>
              ))}
            </select>

            <input
              type="time"
              value={scheduleTime}
              onChange={(event) =>
                setScheduleTime(event.target.value)
              }
              disabled={
                !enabled || schedule !== "MONTHLY"
              }
              className="rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100 disabled:bg-gray-100 disabled:text-gray-400"
            />
          </label>
        </div>

        <div className="mt-5 flex justify-end">
          <button
            type="button"
            onClick={saveSchedule}
            disabled={saving}
            className="rounded-lg border border-gray-300 bg-white px-5 py-2.5 text-sm font-medium text-gray-700 shadow-sm transition hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {saving
              ? "Menyimpan..."
              : "Simpan Jadwal"}
          </button>
        </div>
      </div>

      <div className="py-5">
        <div className="text-sm font-semibold text-gray-900">
          Status Sinkronisasi
        </div>

        <div className="mt-4 grid grid-cols-1 gap-4 md:grid-cols-3">
          <div className="rounded-lg bg-gray-50 p-4">
            <div className="text-xs font-medium uppercase tracking-wide text-gray-500">
              Mesin Aktif
            </div>

            <div className="mt-1 text-2xl font-semibold text-gray-900">
              {activeMachines.length}
            </div>
          </div>

          <div className="rounded-lg bg-gray-50 p-4">
            <div className="text-xs font-medium uppercase tracking-wide text-gray-500">
              Sinkronisasi Terakhir
            </div>

            <div className="mt-1 text-sm font-semibold text-gray-900">
              {lastScheduledSync
                ? new Date(lastScheduledSync).toLocaleString(
                    "id-ID",
                    {
                      dateStyle: "medium",
                      timeStyle: "short",
                    },
                  )
                : lastSync ?? "Belum pernah"}
            </div>
          </div>

          <div className="rounded-lg bg-gray-50 p-4">
            <div className="text-xs font-medium uppercase tracking-wide text-gray-500">
              Sinkronisasi Berikutnya
            </div>

            <div className="mt-1 text-sm font-semibold text-gray-900">
              {loadingSchedule
                ? "Memuat..."
                : nextSyncAt
                  ? new Date(nextSyncAt).toLocaleString(
                      "id-ID",
                      {
                        dateStyle: "medium",
                        timeStyle: "short",
                      },
                    )
                  : "Tidak dijadwalkan"}
            </div>
          </div>
        </div>

        <div className="mt-5 flex justify-end">
          <button
            type="button"
            onClick={syncNow}
            disabled={
              syncing ||
              activeMachines.length === 0
            }
            className="rounded-lg bg-blue-600 px-5 py-2.5 text-sm font-medium text-white shadow-sm transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {syncing
              ? "Sinkronisasi..."
              : "🔄 Sinkronisasi Sekarang"}
          </button>
        </div>
      </div>

      {message && (
        <div className="border-t border-gray-100 pt-4">
          <div className="rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
            {message}
          </div>
        </div>
      )}

      {error && (
        <div className="border-t border-gray-100 pt-4">
          <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {error}
          </div>
        </div>
      )}

      {results.length > 0 && (
        <div className="border-t border-gray-100 pt-5">
          <div className="text-sm font-semibold text-gray-900">
            Hasil Sinkronisasi Terakhir
          </div>

          <div className="mt-3 overflow-x-auto rounded-lg border border-gray-200">
            <table className="min-w-full divide-y divide-gray-200 text-sm">
              <thead className="bg-gray-50">
                <tr>
                  <th className="px-4 py-3 text-left font-semibold text-gray-600">
                    Mesin
                  </th>

                  <th className="px-4 py-3 text-center font-semibold text-gray-600">
                    Status
                  </th>

                  <th className="px-4 py-3 text-center font-semibold text-gray-600">
                    Cocok
                  </th>

                  <th className="px-4 py-3 text-center font-semibold text-gray-600">
                    Dibuat
                  </th>

                  <th className="px-4 py-3 text-center font-semibold text-gray-600">
                    Diubah
                  </th>

                  <th className="px-4 py-3 text-center font-semibold text-gray-600">
                    Disabled
                  </th>

                  <th className="px-4 py-3 text-center font-semibold text-gray-600">
                    Dihapus
                  </th>
                </tr>
              </thead>

              <tbody className="divide-y divide-gray-100 bg-white">
                {results.map((result) => (
                  <tr key={result.machineCode}>
                    <td className="px-4 py-3">
                      <div className="font-medium text-gray-900">
                        {result.machineCode}
                      </div>

                      <div className="text-xs text-gray-500">
                        {result.machineName}
                      </div>
                    </td>

                    <td className="px-4 py-3 text-center">
                      <span
                        className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold ${
                          result.status === "SUCCESS"
                            ? "bg-green-100 text-green-700"
                            : result.status === "PENDING"
                              ? "bg-yellow-100 text-yellow-700"
                              : "bg-red-100 text-red-700"
                        }`}
                      >
                        {result.status}
                      </span>
                    </td>

                    <td className="px-4 py-3 text-center">
                      {result.matched}
                    </td>

                    <td className="px-4 py-3 text-center">
                      {result.created}
                    </td>

                    <td className="px-4 py-3 text-center">
                      {result.updated}
                    </td>

                    <td className="px-4 py-3 text-center">
                      {result.disabled}
                    </td>

                    <td className="px-4 py-3 text-center">
                      {result.deleted}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </section>
  );
}
