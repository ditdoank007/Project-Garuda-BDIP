"use client";

import { useEffect, useMemo, useState } from "react";
import {
  getAttendanceMaster,
  saveAttendanceMaster,
  setAttendanceMachineUserEnabled,
} from "@/lib/api/attendance";
import type { AttendanceMasterUser } from "@/types/attendance";
import type { User } from "@/types/users";
import { getUsers } from "@/services/users.service";

export default function AttendanceMasterClient() {
  const [users, setUsers] = useState<AttendanceMasterUser[]>([]);
  const [bdipUsers, setBdipUsers] = useState<User[]>([]);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [changingMachineUser, setChangingMachineUser] = useState<string | null>(null);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [search, setSearch] = useState("");

  const [editing, setEditing] = useState<AttendanceMasterUser | null>(null);
  const [editName, setEditName] = useState("");
  const [selectedUserId, setSelectedUserId] = useState("");

  async function loadMaster() {
    try {
      setLoading(true);
      setError("");

      const response = await getAttendanceMaster();
      setUsers(response.users);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Gagal mengambil Master Attendance.",
      );
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadMaster();

    async function loadBdipUsers() {
      try {
        const response = await getUsers();
        setBdipUsers(response.data.users);
      } catch (err) {
        setError(
          err instanceof Error
            ? err.message
            : "Gagal mengambil daftar User BDIP.",
        );
      }
    }

    void loadBdipUsers();
  }, []);

  const filteredUsers = useMemo(() => {
    const keyword = search.trim().toLowerCase();

    if (!keyword) {
      return users;
    }

    return users.filter((user) => {
      const machineText = user.machines
        .map((machine) => `${machine.machineCode} ${machine.machineName}`)
        .join(" ");

      return [
        user.fingerId,
        user.fullName,
        user.nip,
        machineText,
      ]
        .join(" ")
        .toLowerCase()
        .includes(keyword);
    });
  }, [users, search]);

  const totalUsers = users.length;

  const activeUsers = users.filter(
    (user) => user.userEnabled,
  ).length;

  const totalFingerprints = users.reduce(
    (total, user) => total + user.fingerprintCount,
    0,
  );

  const totalRegistrations = users.reduce(
    (total, user) =>
      total +
      user.machines.filter((machine) => machine.registered).length,
    0,
  );

  function openEdit(user: AttendanceMasterUser) {
    setSuccess("");
    setError("");
    setEditing(user);
    setEditName(user.fullName);
    setSelectedUserId(user.isLinked ? user.userId : "");
  }

  function closeEdit() {
    if (saving || changingMachineUser) {
      return;
    }

    setEditing(null);
    setEditName("");
    setSelectedUserId("");
  }

  async function handleMachineUserEnabled(
    user: AttendanceMasterUser,
    machine: AttendanceMasterUser["machines"][number],
  ) {
    if (!machine.registered) {
      return;
    }

    const actionKey = `${user.fingerId}-${machine.machineCode}`;

    try {
      setChangingMachineUser(actionKey);
      setError("");
      setSuccess("");

      const nextEnabled = !machine.enabled;

      await setAttendanceMachineUserEnabled(
        machine.machineCode,
        machine.deviceUid,
        nextEnabled,
      );

      setSuccess(
        `FingerID ${user.fingerId} berhasil ${
          nextEnabled ? "diaktifkan" : "dinonaktifkan"
        } pada ${machine.machineCode}.`,
      );

      await loadMaster();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Gagal mengubah status user pada Finger Machine.",
      );
    } finally {
      setChangingMachineUser(null);
    }
  }

  async function handleSave() {
    if (!editing) {
      return;
    }

    const name = editName.trim();

    if (!name) {
      setError("Nama Lengkap wajib diisi.");
      return;
    }

    try {
      setSaving(true);
      setError("");
      setSuccess("");

      await saveAttendanceMaster(
        editing.fingerId,
        name,
        selectedUserId || undefined,
      );

      setEditing(null);
      setEditName("");
      setSelectedUserId("");
      setSuccess(
        `FingerID ${editing.fingerId} berhasil disimpan sebagai ${name}.`,
      );

      await loadMaster();
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : "Gagal menyimpan Master Attendance.",
      );
    } finally {
      setSaving(false);
    }
  }

  return (
    <section className="space-y-6">
      <div>
        <h2 className="text-xl font-semibold text-gray-900">
          Master Attendance
        </h2>
        <p className="mt-1 text-sm text-gray-500">
          Data pengguna fingerprint yang menjadi master distribusi ke mesin.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-4 md:grid-cols-4">
        <SummaryCard
          label="Total User"
          value={totalUsers}
        />
        <SummaryCard
          label="User Aktif"
          value={activeUsers}
        />
        <SummaryCard
          label="Fingerprint"
          value={totalFingerprints}
        />
        <SummaryCard
          label="Registrasi Mesin"
          value={totalRegistrations}
        />
      </div>

      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      {success && (
        <div className="rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
          {success}
        </div>
      )}

      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div className="text-sm text-gray-500">
          Menampilkan {filteredUsers.length} dari {totalUsers} user
        </div>

        <input
          type="search"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
          placeholder="Cari FingerID, nama, NIP, atau mesin..."
          className="w-full rounded-lg border border-gray-300 px-4 py-2 text-sm outline-none focus:border-gray-500 sm:w-96"
        />
      </div>

      <div className="overflow-hidden rounded-xl border border-gray-200 bg-white">
        <div className="overflow-x-auto">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                  FingerID
                </th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                  Nama Lengkap
                </th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                  NIP
                </th>
                <th className="px-4 py-3 text-center text-xs font-semibold uppercase tracking-wide text-gray-500">
                  Fingerprint
                </th>
                <th className="px-4 py-3 text-center text-xs font-semibold uppercase tracking-wide text-gray-500">
                  Status BDIP
                </th>
                <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-gray-500">
                  Mesin
                </th>
                <th className="px-4 py-3 text-center text-xs font-semibold uppercase tracking-wide text-gray-500">
                  Status Mesin
                </th>
                <th className="px-4 py-3 text-center text-xs font-semibold uppercase tracking-wide text-gray-500">
                  Aksi
                </th>
              </tr>
            </thead>

            <tbody className="divide-y divide-gray-100 bg-white">
              {loading ? (
                <tr>
                  <td
                    colSpan={8}
                    className="px-4 py-10 text-center text-sm text-gray-500"
                  >
                    Memuat Master Attendance...
                  </td>
                </tr>
              ) : filteredUsers.length === 0 ? (
                <tr>
                  <td
                    colSpan={8}
                    className="px-4 py-10 text-center text-sm text-gray-500"
                  >
                    Data tidak ditemukan.
                  </td>
                </tr>
              ) : (
                filteredUsers.map((user) => (
                  <tr
                    key={`${user.fingerId}-${user.userId}`}
                    className="hover:bg-gray-50"
                  >
                    <td className="whitespace-nowrap px-4 py-3 text-sm font-medium text-gray-900">
                      {user.fingerId}
                    </td>

                    <td className="px-4 py-3 text-sm text-gray-900">
                      {user.fullName}
                    </td>

                    <td className="whitespace-nowrap px-4 py-3 text-sm text-gray-600">
                      {user.nip || "-"}
                    </td>

                    <td className="px-4 py-3 text-center text-sm text-gray-700">
                      {user.fingerprintCount}
                    </td>

                    <td className="px-4 py-3 text-center">
                      <StatusBadge
                        active={user.userEnabled}
                        activeText="Aktif"
                        inactiveText="Nonaktif"
                      />
                    </td>

                    <td className="px-4 py-3 text-sm text-gray-700">
                      {user.machines.length === 0 ? (
                        "-"
                      ) : (
                        <div className="flex flex-wrap gap-1">
                          {user.machines.map((machine) => (
                            <span
                              key={`${user.fingerId}-${machine.machineCode}`}
                              className="rounded-md bg-gray-100 px-2 py-1 text-xs"
                              title={machine.machineName}
                            >
                              {machine.machineCode}
                            </span>
                          ))}
                        </div>
                      )}
                    </td>

                    <td className="px-4 py-3">
                      {user.machines.length === 0 ? (
                        <span className="block text-center text-sm text-gray-400">
                          -
                        </span>
                      ) : (
                        <div className="space-y-2">
                          {user.machines.map((machine) => {
                            const actionKey = `${user.fingerId}-${machine.machineCode}`;
                            const changing =
                              changingMachineUser === actionKey;

                            return (
                              <div
                                key={actionKey}
                                className="flex items-center justify-between gap-3 rounded-lg border border-gray-100 bg-gray-50 px-3 py-2"
                              >
                                <div className="min-w-0">
                                  <div className="text-xs font-medium text-gray-700">
                                    {machine.machineCode}
                                  </div>
                                  <div
                                    className={
                                      machine.enabled
                                        ? "text-xs text-green-600"
                                        : "text-xs text-gray-500"
                                    }
                                  >
                                    {machine.enabled
                                      ? "Aktif"
                                      : "Nonaktif"}
                                  </div>
                                </div>

                                {machine.registered && machine.deviceUid > 0 ? (
                                  <button
                                    type="button"
                                    disabled={
                                      changing ||
                                      changingMachineUser !== null
                                    }
                                    onClick={() =>
                                      void handleMachineUserEnabled(
                                        user,
                                        machine,
                                      )
                                    }
                                    className={
                                      machine.enabled
                                        ? "rounded-md border border-red-200 px-2.5 py-1 text-xs font-medium text-red-600 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-50"
                                        : "rounded-md border border-green-200 px-2.5 py-1 text-xs font-medium text-green-600 hover:bg-green-50 disabled:cursor-not-allowed disabled:opacity-50"
                                    }
                                  >
                                    {changing
                                      ? "Proses..."
                                      : machine.enabled
                                        ? "Nonaktifkan"
                                        : "Aktifkan"}
                                  </button>
                                ) : (
                                  <span className="text-xs text-gray-400">
                                    Belum terdaftar
                                  </span>
                                )}
                              </div>
                            );
                          })}
                        </div>
                      )}
                    </td>

                    <td className="px-4 py-3 text-center">
                      <button
                        type="button"
                        onClick={() => openEdit(user)}
                        className="rounded-lg border border-gray-300 px-3 py-1.5 text-sm font-medium text-gray-700 hover:bg-gray-50"
                      >
                        Edit
                      </button>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {editing && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4">
          <div className="w-full max-w-md rounded-xl bg-white p-6 shadow-xl">
            <div className="flex items-start justify-between gap-4">
              <div>
                <h3 className="text-lg font-semibold text-gray-900">
                  Edit Master Attendance
                </h3>
                <p className="mt-1 text-sm text-gray-500">
                  Lengkapi atau ubah nama pengguna berdasarkan FingerID.
                </p>
              </div>

              <button
                type="button"
                onClick={closeEdit}
                disabled={saving}
                className="text-xl leading-none text-gray-400 hover:text-gray-700"
              >
                ×
              </button>
            </div>

            <div className="mt-6 space-y-4">
              <div>
                <label className="mb-1 block text-sm font-medium text-gray-700">
                  FingerID
                </label>
                <input
                  value={editing.fingerId}
                  disabled
                  className="w-full rounded-lg border border-gray-200 bg-gray-50 px-3 py-2 text-sm text-gray-500"
                />
              </div>

              {!editing.isLinked && (
                <div>
                  <label className="mb-1 block text-sm font-medium text-gray-700">
                    User BDIP
                  </label>
                  <select
                    value={selectedUserId}
                    onChange={(event) => {
                      const userId = event.target.value;
                      setSelectedUserId(userId);

                      const selectedUser = bdipUsers.find(
                        (user) => user.uid === userId,
                      );

                      if (selectedUser) {
                        setEditName(selectedUser.fullName);
                      }
                    }}
                    className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 outline-none focus:border-gray-500"
                  >
                    <option value="">-- Pilih User BDIP --</option>
                    {bdipUsers
                      .slice()
                      .sort((a, b) =>
                        a.fullName.localeCompare(b.fullName),
                      )
                      .map((user) => (
                        <option key={user.uid} value={user.uid}>
                          {user.fullName}
                          {user.nip ? ` — ${user.nip}` : ""}
                        </option>
                      ))}
                  </select>
                </div>
              )}

              <div>
                <label className="mb-1 block text-sm font-medium text-gray-700">
                  Nama Lengkap
                </label>
                <input
                  value={editName}
                  onChange={(event) => setEditName(event.target.value)}
                  disabled={!editing.isLinked && !selectedUserId}
                  autoFocus={editing.isLinked}
                  className="w-full rounded-lg border border-gray-300 px-3 py-2 text-sm text-gray-900 outline-none focus:border-gray-500 disabled:bg-gray-50 disabled:text-gray-500"
                  placeholder="Masukkan nama lengkap"
                  onKeyDown={(event) => {
                    if (event.key === "Enter" && !saving) {
                      void handleSave();
                    }
                  }}
                />
              </div>
            </div>

            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={closeEdit}
                disabled={saving}
                className="rounded-lg border border-gray-300 px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50 disabled:opacity-50"
              >
                Batal
              </button>

              <button
                type="button"
                onClick={() => void handleSave()}
                disabled={
                  saving ||
                  !editName.trim() ||
                  (!editing.isLinked && !selectedUserId)
                }
                className="rounded-lg bg-gray-900 px-4 py-2 text-sm font-medium text-white hover:bg-gray-800 disabled:cursor-not-allowed disabled:opacity-50"
              >
                {saving ? "Menyimpan..." : "Simpan"}
              </button>
            </div>
          </div>
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
    <div className="rounded-xl border border-gray-200 bg-white p-5">
      <div className="text-sm text-gray-500">{label}</div>
      <div className="mt-2 text-2xl font-semibold text-gray-900">
        {value}
      </div>
    </div>
  );
}

function StatusBadge({
  active,
  activeText,
  inactiveText,
}: {
  active: boolean;
  activeText: string;
  inactiveText: string;
}) {
  return (
    <span
      className={
        active
          ? "rounded-full bg-green-100 px-2.5 py-1 text-xs font-medium text-green-700"
          : "rounded-full bg-gray-100 px-2.5 py-1 text-xs font-medium text-gray-500"
      }
    >
      {active ? activeText : inactiveText}
    </span>
  );
}
