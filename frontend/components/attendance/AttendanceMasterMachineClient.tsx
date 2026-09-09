"use client";

import { useEffect, useMemo, useState } from "react";

import {
  getAttendanceMasterMachine,
  updateAttendanceMasterMachine,
} from "@/lib/api/attendance";
import type { FingerMachine } from "@/types/finger-machine";

interface AttendanceMasterMachineClientProps {
  machines: FingerMachine[];
}

export default function AttendanceMasterMachineClient({
  machines,
}: AttendanceMasterMachineClientProps) {
  const [masterMachineId, setMasterMachineId] = useState("");
  const [savedMachineId, setSavedMachineId] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const activeMachines = useMemo(
    () => machines.filter((machine) => machine.isActive),
    [machines],
  );

  useEffect(() => {
    let cancelled = false;

    async function loadMasterMachine() {
      try {
        setLoading(true);
        setError("");

        const response =
          await getAttendanceMasterMachine();

        const machineId =
          response?.data?.machineId ?? "";

        if (!cancelled) {
          setMasterMachineId(machineId);
          setSavedMachineId(machineId);
        }
      } catch (exception) {
        if (!cancelled) {
          setError(
            exception instanceof Error
              ? exception.message
              : "Gagal memuat Master Mesin Finger.",
          );
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    loadMasterMachine();

    return () => {
      cancelled = true;
    };
  }, []);

  async function handleSave() {
    if (!masterMachineId) {
      setError("Pilih Master Mesin Finger terlebih dahulu.");
      return;
    }

    try {
      setSaving(true);
      setMessage("");
      setError("");

      const response =
        await updateAttendanceMasterMachine(
          masterMachineId,
        );

      const savedId =
        response?.data?.machineId ?? masterMachineId;

      setMasterMachineId(savedId);
      setSavedMachineId(savedId);
      setMessage(
        "Master Mesin Finger berhasil disimpan.",
      );
    } catch (exception) {
      setError(
        exception instanceof Error
          ? exception.message
          : "Gagal menyimpan Master Mesin Finger.",
      );
    } finally {
      setSaving(false);
    }
  }

  const selectedMachine = activeMachines.find(
    (machine) =>
      machine.id === masterMachineId,
  );

  return (
    <section className="rounded-xl border border-gray-200 bg-white p-6 shadow-sm">
      <div className="mb-5">
        <h2 className="text-lg font-semibold text-gray-900">
          Master Mesin Finger
        </h2>

        <p className="mt-1 text-sm text-gray-500">
          Mesin yang dipilih menjadi sumber utama data
          user dan fingerprint untuk Master Attendance.
          Mesin lainnya digunakan sebagai mesin target
          dan reconciliation.
        </p>
      </div>

      <div className="max-w-2xl">
        <label
          htmlFor="attendance-master-machine"
          className="mb-2 block text-sm font-medium text-gray-700"
        >
          Master Mesin Finger
        </label>

        <div className="flex flex-col gap-3 sm:flex-row">
          <select
            id="attendance-master-machine"
            value={masterMachineId}
            onChange={(event) => {
              setMasterMachineId(event.target.value);
              setMessage("");
              setError("");
            }}
            disabled={
              loading ||
              saving ||
              activeMachines.length === 0
            }
            className="w-full rounded-lg border border-gray-300 bg-white px-3 py-2.5 text-sm text-gray-900 outline-none transition focus:border-gray-500 focus:ring-2 focus:ring-gray-200 disabled:bg-gray-100"
          >
            <option value="">
              {loading
                ? "Memuat Master Mesin..."
                : "Pilih Master Mesin Finger"}
            </option>

            {activeMachines.map((machine) => (
              <option
                key={machine.id}
                value={machine.id}
              >
                {machine.code} — {machine.name}
              </option>
            ))}
          </select>

          <button
            type="button"
            onClick={handleSave}
            disabled={
              loading ||
              saving ||
              !masterMachineId ||
              masterMachineId === savedMachineId
            }
            className="rounded-lg bg-gray-900 px-5 py-2.5 text-sm font-medium text-white transition hover:bg-gray-800 disabled:cursor-not-allowed disabled:bg-gray-300"
          >
            {saving ? "Menyimpan..." : "Simpan"}
          </button>
        </div>

        {selectedMachine && (
          <p className="mt-2 text-xs text-gray-500">
            Master saat ini:{" "}
            <span className="font-medium text-gray-700">
              {selectedMachine.code} —{" "}
              {selectedMachine.name}
            </span>
          </p>
        )}

        {message && (
          <p className="mt-3 text-sm text-green-600">
            {message}
          </p>
        )}

        {error && (
          <p className="mt-3 text-sm text-red-600">
            {error}
          </p>
        )}
      </div>
    </section>
  );
}
