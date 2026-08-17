"use client";

import {
  Activity,
  Cpu,
  Database,
  Fan,
  HardDrive,
  MemoryStick,
  Thermometer,
} from "lucide-react";

import type { SynologyMonitoring } from "@/types/dashboard";

function formatBytes(bytes: number) {
  if (!bytes || bytes <= 0) return "0 B";

  const units = ["B", "KB", "MB", "GB", "TB", "PB"];
  const index = Math.min(
    Math.floor(Math.log(bytes) / Math.log(1024)),
    units.length - 1,
  );

  return `${(bytes / Math.pow(1024, index)).toFixed(2)} ${units[index]}`;
}

function formatRate(bytesPerSecond: number | null | undefined) {
  if (bytesPerSecond == null) return "—";

  return `${formatBytes(bytesPerSecond)}/s`;
}

function formatIops(value: number | null | undefined) {
  if (value == null) return "—";

  return Math.round(value).toLocaleString();
}

function formatPercent(value: number | null | undefined) {
  if (value == null) return "—";

  return `${value.toFixed(0)}%`;
}

function formatTemperature(value: number | null | undefined) {
  if (value == null) return "—";

  return `${value.toFixed(0)}°C`;
}

function StatusValue({
  value,
  fallback = "—",
}: {
  value: string | null | undefined;
  fallback?: string;
}) {
  return (
    <span className="font-medium uppercase text-emerald-400">
      {value || fallback}
    </span>
  );
}

export default function SynologyStorageCard({
  synology,
}: {
  synology: SynologyMonitoring;
}) {
  const usedPercent = synology.usedPercent ?? 0;
  const freePercent = Math.max(0, 100 - usedPercent);

  const performance = synology.performance;
  const resources = synology.systemResources;
  const storageHealth = synology.storageHealth;

  const diskHealth =
    storageHealth?.diskHealth ??
    `${synology.hardware.healthyDisks}/${synology.hardware.diskCount} OK`;

  const raidStatus =
    storageHealth?.raidStatus ??
    synology.hardware.poolStatus ??
    synology.status;

  const filesystemStatus =
    storageHealth?.filesystemStatus ??
    synology.fileSystem;

  return (
    <section className="rounded-xl border border-slate-800/60 bg-[#111b2d] p-5">
      <div className="mb-5 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="rounded-lg bg-emerald-600/20 p-2">
            <Database className="h-5 w-5 text-emerald-400" />
          </div>

          <div>
            <h2 className="text-lg font-semibold text-white">
              SYNOLOGY STORAGE
            </h2>

            <p className="text-xs text-slate-400">
              {synology.model} • {synology.dsmVersion}
            </p>
          </div>
        </div>

        <span
          className={`rounded-full px-3 py-1 text-xs font-semibold ${
            synology.online
              ? "bg-emerald-500/15 text-emerald-400"
              : "bg-red-500/15 text-red-400"
          }`}
        >
          {synology.online ? "ONLINE" : "OFFLINE"}
        </span>
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        {/* CAPACITY */}
        <div className="rounded-lg border border-slate-800 bg-[#0c1627] p-5">
          <div className="mb-3 text-xs font-semibold uppercase tracking-wider text-slate-500">
            Capacity
          </div>

          <div className="grid gap-4 sm:grid-cols-[180px_1fr] lg:grid-cols-1 xl:grid-cols-[180px_1fr]">
            <div className="flex items-center justify-center">
              <div className="relative flex h-40 w-40 items-center justify-center">
                <svg
                  className="absolute inset-0 h-full w-full -rotate-90"
                  viewBox="0 0 120 120"
                >
                  <circle
                    cx="60"
                    cy="60"
                    r="48"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="10"
                    className="text-slate-700"
                  />

                  <circle
                    cx="60"
                    cy="60"
                    r="48"
                    fill="none"
                    stroke="currentColor"
                    strokeWidth="10"
                    strokeLinecap="round"
                    strokeDasharray={`${usedPercent * 3.0159} 301.59`}
                    className={
                      usedPercent >= 85
                        ? "text-red-500"
                        : usedPercent >= 70
                          ? "text-amber-400"
                          : "text-emerald-400"
                    }
                  />
                </svg>

                <div className="text-center">
                  <div className="text-3xl font-bold text-white">
                    {usedPercent.toFixed(1)}%
                  </div>

                  <div className="text-xs uppercase tracking-wider text-slate-400">
                    Used
                  </div>
                </div>
              </div>
            </div>

            <div className="grid gap-3">
              <div className="rounded-lg bg-[#111d31] p-3">
                <div className="text-xs text-slate-500">Used</div>
                <div className="mt-1 text-xl font-bold text-white">
                  {formatBytes(synology.usedBytes)}
                </div>
              </div>

              <div className="rounded-lg bg-[#111d31] p-3">
                <div className="text-xs text-slate-500">Free</div>
                <div className="mt-1 text-xl font-bold text-white">
                  {formatBytes(synology.freeBytes)}
                </div>
                <div className="mt-1 text-xs text-sky-400">
                  {freePercent.toFixed(2)}%
                </div>
              </div>
            </div>
          </div>

          <div className="mt-4 border-t border-slate-800 pt-3">
            <div className="text-sm text-slate-300">
              {synology.volumeName}
            </div>

            <div className="text-xs text-slate-500">
              {synology.volumePath}
            </div>
          </div>
        </div>

        {/* PERFORMANCE */}
        <div className="rounded-lg border border-slate-800 bg-[#0c1627] p-5">
          <div className="mb-4 flex items-center gap-2">
            <Activity className="h-4 w-4 text-sky-400" />

            <div className="text-xs font-semibold uppercase tracking-wider text-slate-500">
              Performance
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="rounded-lg bg-[#111d31] p-4">
              <div className="text-xs text-slate-500">READ</div>

              <div className="mt-1 text-xl font-bold text-white">
                {formatRate(performance?.readBytesPerSecond)}
              </div>
            </div>

            <div className="rounded-lg bg-[#111d31] p-4">
              <div className="text-xs text-slate-500">WRITE</div>

              <div className="mt-1 text-xl font-bold text-white">
                {formatRate(performance?.writeBytesPerSecond)}
              </div>
            </div>

            <div className="rounded-lg bg-[#111d31] p-4">
              <div className="text-xs text-slate-500">READ IOPS</div>

              <div className="mt-1 text-xl font-bold text-white">
                {formatIops(performance?.readIops)}
              </div>
            </div>

            <div className="rounded-lg bg-[#111d31] p-4">
              <div className="text-xs text-slate-500">WRITE IOPS</div>

              <div className="mt-1 text-xl font-bold text-white">
                {formatIops(performance?.writeIops)}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* SYSTEM + STORAGE HEALTH */}
      <div className="mt-4 grid gap-4 lg:grid-cols-2">
        <div className="rounded-lg border border-slate-800 bg-[#0c1627] p-4">
          <div className="mb-4 text-xs font-semibold uppercase tracking-wider text-slate-500">
            System
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="rounded-lg bg-[#111d31] p-3">
              <div className="flex items-center gap-2 text-xs text-slate-500">
                <Cpu className="h-3.5 w-3.5" />
                CPU
              </div>

              <div className="mt-1 text-lg font-bold text-white">
                {formatPercent(resources?.cpuPercent)}
              </div>
            </div>

            <div className="rounded-lg bg-[#111d31] p-3">
              <div className="flex items-center gap-2 text-xs text-slate-500">
                <MemoryStick className="h-3.5 w-3.5" />
                MEMORY
              </div>

              <div className="mt-1 text-lg font-bold text-white">
                {formatPercent(resources?.memoryPercent)}
              </div>
            </div>

            <div className="rounded-lg bg-[#111d31] p-3">
              <div className="flex items-center gap-2 text-xs text-slate-500">
                <Thermometer className="h-3.5 w-3.5" />
                TEMP
              </div>

              <div className="mt-1 text-lg font-bold text-white">
                {formatTemperature(resources?.temperatureC)}
              </div>
            </div>

            <div className="rounded-lg bg-[#111d31] p-3">
              <div className="flex items-center gap-2 text-xs text-slate-500">
                <Fan className="h-3.5 w-3.5" />
                FAN
              </div>

              <div className="mt-1 text-lg font-bold">
                <StatusValue value={resources?.fanStatus} />
              </div>
            </div>
          </div>
        </div>

        <div className="rounded-lg border border-slate-800 bg-[#0c1627] p-4">
          <div className="mb-4 text-xs font-semibold uppercase tracking-wider text-slate-500">
            Storage Health
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="rounded-lg bg-[#111d31] p-3">
              <div className="text-xs text-slate-500">RAID</div>
              <div className="mt-1">
                <StatusValue value={raidStatus} />
              </div>
            </div>

            <div className="rounded-lg bg-[#111d31] p-3">
              <div className="text-xs text-slate-500">FILESYSTEM</div>
              <div className="mt-1">
                <StatusValue value={filesystemStatus} />
              </div>
            </div>

            <div className="rounded-lg bg-[#111d31] p-3">
              <div className="flex items-center gap-2 text-xs text-slate-500">
                <HardDrive className="h-3.5 w-3.5" />
                DISKS
              </div>

              <div className="mt-1 text-lg font-bold text-emerald-400">
                {diskHealth}
              </div>
            </div>

            <div className="rounded-lg bg-[#111d31] p-3">
              <div className="text-xs text-slate-500">BAD SECTOR</div>

              <div className="mt-1 text-lg font-bold text-emerald-400">
                {storageHealth?.badSectors == null
                  ? "—"
                  : storageHealth.badSectors.toLocaleString()}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* EXISTING STORAGE SUMMARY */}
      <div className="mt-4 grid grid-cols-2 gap-3 md:grid-cols-4">
        <div className="rounded-lg bg-[#0c1627] p-3">
          <div className="text-xs text-slate-500">Total Storage</div>
          <div className="mt-1 font-medium text-white">
            {formatBytes(synology.totalBytes)}
          </div>
        </div>

        <div className="rounded-lg bg-[#0c1627] p-3">
          <div className="text-xs text-slate-500">Filesystem</div>
          <div className="mt-1 font-medium uppercase text-white">
            {synology.fileSystem || "-"}
          </div>
        </div>

        <div className="rounded-lg bg-[#0c1627] p-3">
          <div className="text-xs text-slate-500">RAID</div>
          <div className="mt-1 font-medium uppercase text-white">
            {synology.raidType || "-"}
          </div>
        </div>

        <div className="rounded-lg bg-[#0c1627] p-3">
          <div className="text-xs text-slate-500">Storage Usage</div>
          <div className="mt-1 flex items-center gap-2 font-medium text-white">
            <Activity className="h-4 w-4 text-emerald-400" />
            {usedPercent.toFixed(2)}%
          </div>
        </div>
      </div>
    </section>
  );
}
