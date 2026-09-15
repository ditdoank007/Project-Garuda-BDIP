"use client";

import { useMemo, useState } from "react";
import type { FingerMachine } from "@/types/finger-machine";
import {
  discoverAttendance,
  importAttendanceDiscovery,
  hapusAttendanceDiscovery,
} from "@/lib/api/attendance";
import type {
  AttendanceDiscoveryResponse,
  AttendanceDiscoveryUser,
} from "@/types/attendance";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

interface Props {
  machines: FingerMachine[];
}

export default function AttendanceDiscoveryClient({ machines }: Props) {
  const activeMachines = useMemo(
    () => machines.filter((machine) => machine.isActive),
    [machines],
  );

  const [machineCode, setMachineCode] = useState(
    activeMachines[0]?.code ?? "",
  );
  const [result, setResult] = useState<AttendanceDiscoveryResponse | null>(
    null,
  );
  const [selectedFingerIds, setSelectedFingerIds] = useState<string[]>([]);
  const [loading, setLoading] = useState(false);
  const [importing, setImporting] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const newUsers = useMemo(
    () => result?.users.filter((user) => !user.isMatched) ?? [],
    [result],
  );

  const allUsersSelected =
    (result?.users.length ?? 0) > 0 &&
    result!.users.every((user) =>
      selectedFingerIds.includes(user.fingerId),
    );

  const allNewSelected =
    newUsers.length > 0 &&
    newUsers.every((user) =>
      selectedFingerIds.includes(user.fingerId),
    );

  const selectedNewCount =
    result?.users.filter(
      (user) =>
        !user.isMatched &&
        selectedFingerIds.includes(user.fingerId),
    ).length ?? 0;

  async function handleDiscover() {
    if (!machineCode) {
      setError("Pilih mesin terlebih dahulu.");
      return;
    }

    setLoading(true);
    setError("");
    setMessage("");
    setSelectedFingerIds([]);

    try {
      const data = await discoverAttendance(machineCode);
      setResult(data);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Gagal membaca data dari mesin.",
      );
      setResult(null);
    } finally {
      setLoading(false);
    }
  }

  function toggleFingerId(fingerId: string) {
    setSelectedFingerIds((current) =>
      current.includes(fingerId)
        ? current.filter((value) => value !== fingerId)
        : [...current, fingerId],
    );
  }

  function toggleAllUsers() {
    if (allUsersSelected) {
      setSelectedFingerIds([]);
      return;
    }

    setSelectedFingerIds(
      result?.users.map((user) => user.fingerId) ?? [],
    );
  }

  function selectAllNewUsers() {
    setSelectedFingerIds(
      newUsers.map((user) => user.fingerId),
    );
  }

  function clearSelection() {
    setSelectedFingerIds([]);
  }

  async function handleImport() {
    if (!machineCode || selectedNewCount === 0) {
      return;
    }

    const selectedNewFingerIds =
      result?.users
        .filter(
          (user) =>
            !user.isMatched &&
            selectedFingerIds.includes(user.fingerId),
        )
        .map((user) => user.fingerId) ?? [];

    const confirmed = window.confirm(
      `Import ${selectedNewFingerIds.length} user baru beserta seluruh fingerprint template ke Master Attendance?`,
    );

    if (!confirmed) {
      return;
    }

    setImporting(true);
    setError("");
    setMessage("");

    try {
      const data = await importAttendanceDiscovery(
        machineCode,
        selectedNewFingerIds,
      );

      setMessage(
        `Berhasil import ${data.importedUserCount} user dan ${data.importedTemplateCount} fingerprint template.`,
      );

      setSelectedFingerIds([]);

      const refreshed = await discoverAttendance(machineCode);
      setResult(refreshed);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Gagal meng-import user Discovery.",
      );
    } finally {
      setImporting(false);
    }
  }

  async function handleHapusDariMesin() {
    if (!machineCode || selectedFingerIds.length === 0) {
      return;
    }

    const confirmed = window.confirm(
      `Hapus ${selectedFingerIds.length} user beserta seluruh fingerprint dari mesin ${machineCode}?\n\nData Master Attendance BDIP TIDAK akan dihapus.`,
    );

    if (!confirmed) {
      return;
    }

    setDeleting(true);
    setError("");
    setMessage("");

    try {
      const data = await hapusAttendanceDiscovery(
        machineCode,
        selectedFingerIds,
      );

      if (data.failedCount === 0) {
        setMessage(
          `Berhasil hapus ${data.deletedCount} user dari mesin. ${data.skippedCount} user dilewati karena sudah tidak ditemukan pada mesin.`,
        );
      } else {
        setMessage(
          `Hapus dari Mesin selesai: ${data.deletedCount} berhasil, ${data.failedCount} gagal, ${data.skippedCount} dilewati.`,
        );
      }

      setSelectedFingerIds([]);

      const refreshed = await discoverAttendance(machineCode);
      setResult(refreshed);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Gagal menghapus user dari mesin.",
      );
    } finally {
      setDeleting(false);
    }
  }

  return (
    <section className="flex h-full min-h-0 flex-col overflow-hidden rounded-xl border border-gray-200 bg-white shadow-sm">
      <div className="border-b border-gray-200 px-6 py-5">
        <div className="flex flex-col gap-4 lg:flex-row lg:items-end lg:justify-between">
          <div>
            <h2 className="text-xl font-semibold text-gray-900">
              Discovery Mesin
            </h2>
            <p className="mt-1 text-sm text-gray-500">
              Baca data user dan fingerprint dari mesin untuk menemukan user
              yang belum ada di Master Attendance.
            </p>
          </div>

          <div className="flex flex-col gap-2 sm:flex-row sm:items-end">
            <div>
              <label
                htmlFor="attendance-discovery-machine"
                className="mb-1 block text-sm font-medium text-gray-700"
              >
                Mesin Finger
              </label>
              <select
                id="attendance-discovery-machine"
                value={machineCode}
                onChange={(event) => setMachineCode(event.target.value)}
                className="min-w-64 rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-200"
                disabled={loading || importing || deleting}
              >
                <option value="">Pilih mesin...</option>
                {activeMachines.map((machine) => (
                  <option key={machine.id} value={machine.code}>
                    {machine.code} — {machine.name}
                  </option>
                ))}
              </select>
            </div>

            <button
              type="button"
              onClick={handleDiscover}
              disabled={!machineCode || loading || importing || deleting}
              className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:cursor-not-allowed disabled:opacity-50"
            >
              {loading ? "Membaca Mesin..." : "Baca Mesin"}
            </button>
          </div>
        </div>
      </div>

      {error && (
        <div className="mx-6 mt-5 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {message && (
        <div className="mx-6 mt-5 rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
          {message}
        </div>
      )}

      {result && (
        <>
          <div className="grid grid-cols-2 gap-4 px-6 py-5 lg:grid-cols-4">
            <SummaryCard
              label="User Mesin"
              value={result.deviceUserCount}
            />
            <SummaryCard
              label="Fingerprint"
              value={result.deviceTemplateCount}
            />
            <SummaryCard
              label="Sudah Terdaftar"
              value={result.matchedUserCount}
            />
            <SummaryCard
              label="User Baru"
              value={result.newUserCount}
            />
          </div>

          <div className="flex min-h-0 flex-1 flex-col overflow-hidden border-t border-gray-200">
            <div className="flex flex-col gap-3 px-6 py-4 lg:flex-row lg:items-center lg:justify-between">
              <div>
                <p className="font-medium text-gray-900">
                  {result.machineName}
                </p>
                <p className="text-sm text-gray-500">
                  {selectedFingerIds.length} user dipilih
                  {selectedNewCount > 0
                    ? ` · ${selectedNewCount} user baru`
                    : ""}
                </p>
              </div>

              <div className="flex flex-wrap gap-2">
                <button
                  type="button"
                  onClick={toggleAllUsers}
                  disabled={
                    result.users.length === 0 ||
                    importing ||
                    deleting
                  }
                  className="rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  {allUsersSelected
                    ? "Batalkan Semua"
                    : "Pilih Semua User"}
                </button>

                <button
                  type="button"
                  onClick={selectAllNewUsers}
                  disabled={
                    newUsers.length === 0 ||
                    importing ||
                    deleting
                  }
                  className="rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  Pilih User Baru
                </button>

                <button
                  type="button"
                  onClick={clearSelection}
                  disabled={
                    selectedFingerIds.length === 0 ||
                    importing ||
                    deleting
                  }
                  className="rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  Batalkan Semua
                </button>

                <button
                  type="button"
                  onClick={handleImport}
                  disabled={
                    selectedNewCount === 0 ||
                    importing ||
                    deleting ||
                    loading
                  }
                  className="rounded-lg bg-green-600 px-4 py-2 text-sm font-medium text-white hover:bg-green-700 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  {importing
                    ? "Meng-import..."
                    : `Import Terpilih (${selectedNewCount})`}
                </button>

                <button
                  type="button"
                  onClick={handleHapusDariMesin}
                  disabled={
                    selectedFingerIds.length === 0 ||
                    deleting ||
                    importing ||
                    loading
                  }
                  className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700 disabled:cursor-not-allowed disabled:opacity-50"
                >
                  {deleting
                    ? "Menghapus dari Mesin..."
                    : `Hapus dari Mesin (${selectedFingerIds.length})`}
                </button>
              </div>
            </div>

            <div className="min-h-0 flex-1 overflow-auto">
              <Table
                containerClassName="overflow-visible"
                className="min-w-[900px]"
              >
                <TableHeader>
                  <TableRow>
                    <TableHead className="sticky top-0 z-20 bg-gray-50 w-12 text-center">
                      <span className="sr-only">Pilih</span>
                    </TableHead>
                    <TableHead className="sticky top-0 z-20 bg-gray-50">
                      FingerID
                    </TableHead>
                    <TableHead className="sticky top-0 z-20 bg-gray-50">
                      Nama Mesin
                    </TableHead>
                    <TableHead className="sticky top-0 z-20 bg-gray-50">
                      Nama BDIP
                    </TableHead>
                    <TableHead className="sticky top-0 z-20 bg-gray-50 text-center">
                      Fingerprint
                    </TableHead>
                    <TableHead className="sticky top-0 z-20 bg-gray-50 text-center">
                      Status
                    </TableHead>
                  </TableRow>
                </TableHeader>

                <TableBody>
                  {result.users.map((user) => (
                    <DiscoveryRow
                      key={`${user.fingerId}-${user.deviceUid}`}
                      user={user}
                      selected={selectedFingerIds.includes(user.fingerId)}
                      onToggle={() => toggleFingerId(user.fingerId)}
                    />
                  ))}
                </TableBody>
              </Table>
            </div>
          </div>
        </>
      )}

      {!result && !loading && !error && (
        <div className="px-6 py-10 text-center text-sm text-gray-500">
          Pilih mesin kemudian klik <strong>Baca Mesin</strong> untuk memulai
          Discovery.
        </div>
      )}
    </section>
  );
}

function SummaryCard({
  label,
  value,
}: {
  label: string;
  value: number;
}) {
  return (
    <div className="rounded-lg border border-gray-200 bg-gray-50 px-4 py-4">
      <p className="text-sm text-gray-500">{label}</p>
      <p className="mt-1 text-2xl font-semibold text-gray-900">{value}</p>
    </div>
  );
}

function DiscoveryRow({
  user,
  selected,
  onToggle,
}: {
  user: AttendanceDiscoveryUser;
  selected: boolean;
  onToggle: () => void;
}) {
  return (
    <TableRow className={user.isMatched ? "" : "bg-amber-50/40"}>
      <TableCell className="px-4 py-3 text-center">
        <input
          type="checkbox"
          checked={selected}
          onChange={onToggle}
          className="h-4 w-4 rounded border-gray-300"
        />
      </TableCell>

      <TableCell className="whitespace-nowrap px-4 py-3 text-sm font-medium text-gray-900">
        {user.fingerId}
      </TableCell>

      <TableCell className="px-4 py-3 text-sm text-gray-700">
        {user.deviceName || "-"}
      </TableCell>

      <TableCell className="px-4 py-3 text-sm text-gray-700">
        {user.bdipFullName || "-"}
      </TableCell>

      <TableCell className="px-4 py-3 text-center text-sm text-gray-700">
        {user.templateCount}
      </TableCell>

      <TableCell className="px-4 py-3 text-center">
        {user.isMatched ? (
          <span className="inline-flex rounded-full bg-green-100 px-2.5 py-1 text-xs font-medium text-green-700">
            Sudah Terdaftar
          </span>
        ) : (
          <span className="inline-flex rounded-full bg-amber-100 px-2.5 py-1 text-xs font-medium text-amber-700">
            User Baru
          </span>
        )}
      </TableCell>
    </TableRow>
  );
}
