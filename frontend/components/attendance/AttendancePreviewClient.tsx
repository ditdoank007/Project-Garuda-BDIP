"use client";

import { useMemo, useState } from "react";

import type { FingerMachine } from "@/types/finger-machine";

interface AttendancePreviewUser {
  deviceUid: number;
  deviceUserId: string;
  deviceName: string;
  fingerId: string;
  matched: boolean;
  templateCount: number;
}

interface AttendancePreviewResponse {
  success: boolean;
  machineCode: string;
  machineName: string;
  deviceUserCount: number;
  deviceTemplateCount: number;
  matchedUserCount: number;
  unmatchedUserCount: number;
  users: AttendancePreviewUser[];
}

interface AttendanceImportResponse {
  success: boolean;
  machineCode: string;
  machineName: string;
  importedUserCount: number;
  importedTemplateCount: number;
  skippedUnmatchedUserCount: number;
}

interface AttendancePreviewClientProps {
  machines: FingerMachine[];
}

export default function AttendancePreviewClient({
  machines,
}: AttendancePreviewClientProps) {
  const activeMachines = useMemo(
    () => machines.filter((machine) => machine.isActive),
    [machines],
  );

  const [machineCode, setMachineCode] = useState(
    activeMachines[0]?.code ?? "",
  );

  const [result, setResult] =
    useState<AttendancePreviewResponse | null>(null);

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [search, setSearch] = useState("");

  const [importing, setImporting] = useState(false);
  const [importResult, setImportResult] =
    useState<AttendanceImportResponse | null>(null);
  const [importError, setImportError] = useState("");

  const selectedMachine = activeMachines.find(
    (machine) => machine.code === machineCode,
  );

  const filteredUsers = useMemo(() => {
    if (!result) {
      return [];
    }

    const keyword = search.trim().toLowerCase();

    if (!keyword) {
      return result.users;
    }

    return result.users.filter((user) =>
      [
        user.deviceUserId,
        user.deviceName,
        user.fingerId,
        String(user.deviceUid),
      ]
        .join(" ")
        .toLowerCase()
        .includes(keyword),
    );
  }, [result, search]);

  async function preview() {
    if (!machineCode) {
      return;
    }

    try {
      setLoading(true);
      setError("");
      setResult(null);

      const response = await fetch("/api/attendance/preview", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          machineCode,
        }),
        cache: "no-store",
      });

      const data =
        (await response.json()) as AttendancePreviewResponse & {
          message?: string;
        };

      if (!response.ok || !data.success) {
        throw new Error(
          data.message || "Attendance Preview gagal.",
        );
      }

      setResult(data);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Attendance Preview gagal.",
      );
    } finally {
      setLoading(false);
    }
  }

  async function importMatched() {
    if (!machineCode || !result || result.matchedUserCount <= 0) {
      return;
    }

    const confirmed = window.confirm(
      `Import ${result.matchedUserCount} user matched dari ${result.machineCode}?`,
    );

    if (!confirmed) {
      return;
    }

    try {
      setImporting(true);
      setImportError("");
      setImportResult(null);

      const response = await fetch("/api/attendance/import", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          machineCode,
        }),
        cache: "no-store",
      });

      const data =
        (await response.json()) as AttendanceImportResponse & {
          message?: string;
        };

      if (!response.ok || !data.success) {
        throw new Error(
          data.message || "Attendance Import gagal.",
        );
      }

      setImportResult(data);
    } catch (err) {
      setImportError(
        err instanceof Error
          ? err.message
          : "Attendance Import gagal.",
      );
    } finally {
      setImporting(false);
    }
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold text-gray-900">
          Attendance
        </h1>
        <p className="mt-1 text-sm text-gray-500">
          Preview data user dan fingerprint dari mesin sebelum
          diproses ke master BDIP.
        </p>
      </div>

      <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
        <div className="flex flex-col gap-4 md:flex-row md:items-end">
          <div className="flex-1">
            <label
              htmlFor="attendance-machine"
              className="mb-2 block text-sm font-medium text-gray-700"
            >
              Mesin Finger
            </label>

            <select
              id="attendance-machine"
              value={machineCode}
              onChange={(event) => {
                setMachineCode(event.target.value);
                setResult(null);
                setError("");
                setImportResult(null);
                setImportError("");
              }}
              className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2.5 text-sm text-gray-900 outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100"
            >
              {activeMachines.length === 0 && (
                <option value="">Tidak ada mesin aktif</option>
              )}

              {activeMachines.map((machine) => (
                <option key={machine.id} value={machine.code}>
                  {machine.code} — {machine.name}
                </option>
              ))}
            </select>
          </div>

          <button
            type="button"
            onClick={preview}
            disabled={!machineCode || loading}
            className="rounded-lg bg-blue-600 px-5 py-2.5 text-sm font-medium text-white shadow-sm transition hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {loading ? "Membaca Mesin..." : "Preview"}
          </button>
        </div>

        {selectedMachine && (
          <div className="mt-4 text-sm text-gray-500">
            {selectedMachine.ipAddress}:{selectedMachine.port}
            {selectedMachine.locationName
              ? ` • ${selectedMachine.locationName}`
              : ""}
          </div>
        )}

        {error && (
          <div className="mt-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {error}
          </div>
        )}
      </div>

      {result && (
        <>
          <div className="rounded-xl border border-gray-200 bg-white p-5 shadow-sm">
            <div className="flex flex-col gap-2 md:flex-row md:items-center md:justify-between">
              <div>
                <h2 className="text-lg font-semibold text-gray-900">
                  {result.machineName}
                </h2>
                <p className="text-sm text-gray-500">
                  {result.machineCode}
                </p>
              </div>

              <span className="inline-flex w-fit rounded-full bg-green-100 px-3 py-1 text-xs font-semibold text-green-700">
                PREVIEW BERHASIL
              </span>
            </div>

            <div className="mt-5 grid grid-cols-2 gap-4 lg:grid-cols-4">
              <div className="rounded-lg bg-gray-50 p-4">
                <div className="text-xs font-medium uppercase tracking-wide text-gray-500">
                  Device Users
                </div>
                <div className="mt-1 text-2xl font-semibold text-gray-900">
                  {result.deviceUserCount}
                </div>
              </div>

              <div className="rounded-lg bg-gray-50 p-4">
                <div className="text-xs font-medium uppercase tracking-wide text-gray-500">
                  Fingerprint
                </div>
                <div className="mt-1 text-2xl font-semibold text-gray-900">
                  {result.deviceTemplateCount}
                </div>
              </div>

              <div className="rounded-lg bg-green-50 p-4">
                <div className="text-xs font-medium uppercase tracking-wide text-green-700">
                  Matched
                </div>
                <div className="mt-1 text-2xl font-semibold text-green-700">
                  {result.matchedUserCount}
                </div>
              </div>

              <div className="rounded-lg bg-amber-50 p-4">
                <div className="text-xs font-medium uppercase tracking-wide text-amber-700">
                  Unmatched
                </div>
                <div className="mt-1 text-2xl font-semibold text-amber-700">
                  {result.unmatchedUserCount}
                </div>
              </div>
            </div>

            <div className="mt-5 flex flex-col gap-3 border-t border-gray-100 pt-5 md:flex-row md:items-center md:justify-between">
              <div>
                <div className="text-sm font-medium text-gray-900">
                  Import ke Master Attendance
                </div>
                <div className="text-sm text-gray-500">
                  Hanya user yang MATCHED yang akan disimpan ke database BDIP.
                </div>
              </div>

              <button
                type="button"
                onClick={importMatched}
                disabled={
                  importing || result.matchedUserCount <= 0
                }
                className="rounded-lg bg-green-600 px-5 py-2.5 text-sm font-medium text-white shadow-sm transition hover:bg-green-700 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {importing
                  ? "Mengimport..."
                  : `Import ${result.matchedUserCount} User Matched`}
              </button>
            </div>

            {importError && (
              <div className="mt-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                {importError}
              </div>
            )}

            {importResult && (
              <div className="mt-4 rounded-lg border border-green-200 bg-green-50 px-4 py-3">
                <div className="text-sm font-semibold text-green-800">
                  IMPORT BERHASIL
                </div>
                <div className="mt-2 grid grid-cols-1 gap-2 text-sm text-green-800 md:grid-cols-3">
                  <div>
                    User di-import:{" "}
                    <span className="font-semibold">
                      {importResult.importedUserCount}
                    </span>
                  </div>
                  <div>
                    Fingerprint disimpan:{" "}
                    <span className="font-semibold">
                      {importResult.importedTemplateCount}
                    </span>
                  </div>
                  <div>
                    Unmatched dilewati:{" "}
                    <span className="font-semibold">
                      {importResult.skippedUnmatchedUserCount}
                    </span>
                  </div>
                </div>
              </div>
            )}
          </div>

          <div className="rounded-xl border border-gray-200 bg-white shadow-sm">
            <div className="flex flex-col gap-3 border-b border-gray-200 p-5 md:flex-row md:items-center md:justify-between">
              <div>
                <h2 className="text-lg font-semibold text-gray-900">
                  User dari Mesin
                </h2>
                <p className="text-sm text-gray-500">
                  {filteredUsers.length} dari {result.users.length} user
                </p>
              </div>

              <input
                type="search"
                value={search}
                onChange={(event) => setSearch(event.target.value)}
                placeholder="Cari FingerID, nama, UID..."
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm outline-none focus:border-blue-500 focus:ring-2 focus:ring-blue-100 md:w-80"
              />
            </div>

            <div className="overflow-x-auto">
              <table className="min-w-full divide-y divide-gray-200 text-sm">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-4 py-3 text-left font-semibold text-gray-600">
                      UID
                    </th>
                    <th className="px-4 py-3 text-left font-semibold text-gray-600">
                      FingerID
                    </th>
                    <th className="px-4 py-3 text-left font-semibold text-gray-600">
                      Nama
                    </th>
                    <th className="px-4 py-3 text-center font-semibold text-gray-600">
                      Fingerprint
                    </th>
                    <th className="px-4 py-3 text-center font-semibold text-gray-600">
                      Status
                    </th>
                  </tr>
                </thead>

                <tbody className="divide-y divide-gray-100 bg-white">
                  {filteredUsers.map((user) => (
                    <tr
                      key={`${user.deviceUid}-${user.deviceUserId}`}
                      className="hover:bg-gray-50"
                    >
                      <td className="whitespace-nowrap px-4 py-3 text-gray-500">
                        {user.deviceUid}
                      </td>

                      <td className="whitespace-nowrap px-4 py-3 font-medium text-gray-900">
                        {user.deviceUserId || "-"}
                      </td>

                      <td className="px-4 py-3 text-gray-700">
                        {user.deviceName || "-"}
                      </td>

                      <td className="px-4 py-3 text-center font-medium text-gray-700">
                        {user.templateCount}
                      </td>

                      <td className="px-4 py-3 text-center">
                        {user.matched ? (
                          <span className="inline-flex rounded-full bg-green-100 px-2.5 py-1 text-xs font-semibold text-green-700">
                            MATCHED
                          </span>
                        ) : (
                          <span className="inline-flex rounded-full bg-gray-100 px-2.5 py-1 text-xs font-semibold text-gray-600">
                            UNMATCHED
                          </span>
                        )}
                      </td>
                    </tr>
                  ))}

                  {filteredUsers.length === 0 && (
                    <tr>
                      <td
                        colSpan={5}
                        className="px-4 py-10 text-center text-sm text-gray-500"
                      >
                        Tidak ada data yang cocok dengan pencarian.
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </>
      )}
    </div>
  );
}
