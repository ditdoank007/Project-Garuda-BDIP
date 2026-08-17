"use client";

import {
  CheckCircle2,
  CircleAlert,
  HardDrive,
  Thermometer,
  XCircle,
  Database,
} from "lucide-react";

import type { SynologyHardware } from "@/types/dashboard";

function formatBytes(bytes: number) {
  if (!bytes || bytes <= 0) return "0 B";

  const units = ["B", "KB", "MB", "GB", "TB", "PB"];
  const index = Math.min(
    Math.floor(Math.log(bytes) / Math.log(1024)),
    units.length - 1,
  );

  return `${(bytes / Math.pow(1024, index)).toFixed(2)} ${units[index]}`;
}

function healthClass(health: string) {
  const value = (health || "").toLowerCase();

  if (
    value.includes("healthy") ||
    value.includes("normal") ||
    value.includes("good")
  ) {
    return "text-emerald-400";
  }

  if (
    value.includes("warning") ||
    value.includes("degrad")
  ) {
    return "text-amber-400";
  }

  if (
    value.includes("fail") ||
    value.includes("critical") ||
    value.includes("error")
  ) {
    return "text-red-400";
  }

  return "text-slate-300";
}

function HealthIcon({ health }: { health: string }) {
  const value = (health || "").toLowerCase();

  if (
    value.includes("fail") ||
    value.includes("critical") ||
    value.includes("error")
  ) {
    return <XCircle className="h-4 w-4 text-red-400" />;
  }

  if (
    value.includes("warning") ||
    value.includes("degrad")
  ) {
    return <CircleAlert className="h-4 w-4 text-amber-400" />;
  }

  return <CheckCircle2 className="h-4 w-4 text-emerald-400" />;
}

export default function SynologyHardwareCard({
  hardware,
}: {
  hardware: SynologyHardware;
}) {
  const overallHealthy =
    hardware.diskCount > 0 &&
    hardware.failedDisks === 0 &&
    hardware.warningDisks === 0;

  return (
    <section className="rounded-xl border border-slate-800/60 bg-[#111b2d] p-5">
      {/* HEADER */}
      <div className="mb-5 flex items-start justify-between gap-4">
        <div className="flex items-center gap-3">
          <div className="rounded-lg bg-blue-600/20 p-2">
            <HardDrive className="h-5 w-5 text-blue-400" />
          </div>

          <div>
            <h2 className="text-lg font-semibold text-white">
              SYNOLOGY HARDWARE
            </h2>

            <p className="text-xs text-slate-400">
              Disk & Hardware Health
            </p>
          </div>
        </div>

        <div
          className={`flex items-center gap-2 rounded-full px-3 py-1.5 text-xs font-semibold ${
            overallHealthy
              ? "bg-emerald-500/10 text-emerald-400"
              : "bg-red-500/10 text-red-400"
          }`}
        >
          <span
            className={`h-2 w-2 rounded-full ${
              overallHealthy ? "bg-emerald-400" : "bg-red-400"
            }`}
          />

          {overallHealthy ? "HEALTHY" : "ATTENTION"}
        </div>
      </div>

      {/* HARDWARE SUMMARY */}
      <div className="mb-5 grid grid-cols-2 gap-3 xl:grid-cols-4">
        <div className="rounded-lg bg-[#0c1627] p-4">
          <div className="text-xs text-slate-500">
            NAS BAYS
          </div>

          <div className="mt-1 text-2xl font-bold text-white">
            {hardware.bayCount}
          </div>

          <div className="mt-1 text-xs text-slate-500">
            Physical Bays
          </div>
        </div>

        <div className="rounded-lg bg-[#0c1627] p-4">
          <div className="text-xs text-slate-500">
            DISKS
          </div>

          <div className="mt-1 text-2xl font-bold text-white">
            {hardware.diskCount}
          </div>

          <div className="mt-1 text-xs text-slate-500">
            Installed
          </div>
        </div>

        <div className="rounded-lg bg-[#0c1627] p-4">
          <div className="text-xs text-slate-500">
            HEALTHY
          </div>

          <div className="mt-1 text-2xl font-bold text-emerald-400">
            {hardware.healthyDisks}
          </div>

          <div className="mt-1 text-xs text-slate-500">
            Normal
          </div>
        </div>

        <div className="rounded-lg bg-[#0c1627] p-4">
          <div className="text-xs text-slate-500">
            ATTENTION
          </div>

          <div className="mt-1 text-2xl font-bold text-white">
            <span className="text-amber-400">
              {hardware.warningDisks}
            </span>

            <span className="mx-1 text-slate-700">
              /
            </span>

            <span className="text-red-400">
              {hardware.failedDisks}
            </span>
          </div>

          <div className="mt-1 text-xs text-slate-500">
            Warning / Failed
          </div>
        </div>
      </div>

      {/* POOL / CACHE */}
      <div className="mb-5 grid gap-3 md:grid-cols-2">
        <div className="rounded-lg border border-slate-800 bg-[#0c1627] p-4">
          <div className="flex items-center gap-2">
            <Database className="h-4 w-4 text-cyan-400" />

            <span className="text-xs font-semibold text-slate-400">
              STORAGE POOL
            </span>
          </div>

          <div className="mt-2 text-sm font-semibold text-white">
            {hardware.poolStatus || "Managed by Volume"}
          </div>

          <div className="mt-1 text-xs text-slate-500">
            {hardware.poolRaidType
              ? `RAID ${hardware.poolRaidType}`
              : "Storage pool information"}
          </div>
        </div>

        <div className="rounded-lg border border-slate-800 bg-[#0c1627] p-4">
          <div className="flex items-center gap-2">
            <Database className="h-4 w-4 text-purple-400" />

            <span className="text-xs font-semibold text-slate-400">
              SSD CACHE
            </span>
          </div>

          <div className="mt-2 text-sm font-semibold text-white">
            {hardware.ssdCache?.enabled ? "ENABLED" : "DISABLED"}
          </div>

          <div className="mt-1 text-xs text-slate-500">
            {hardware.ssdCache?.status
              ? `Status: ${hardware.ssdCache.status}`
              : "No cache information"}
          </div>
        </div>
      </div>

      {/* DISK LIST */}
      <div>
        <div className="mb-3 flex items-center justify-between">
          <div>
            <h3 className="text-sm font-semibold text-white">
              DISK HEALTH
            </h3>

            <p className="text-xs text-slate-500">
              Physical disk condition
            </p>
          </div>

          <div className="text-xs text-slate-500">
            {hardware.disks.length} disks detected
          </div>
        </div>

        <div className="space-y-2">
          {hardware.disks.map((disk) => (
            <div
              key={disk.id}
              className="rounded-lg border border-slate-800 bg-[#0c1627] px-4 py-3"
            >
              <div className="flex flex-col gap-3 lg:flex-row lg:items-center lg:justify-between">
                {/* DISK IDENTITY */}
                <div className="flex min-w-0 items-center gap-3">
                  <div className="rounded-lg bg-slate-800 p-2">
                    <HardDrive className="h-4 w-4 text-slate-300" />
                  </div>

                  <div className="min-w-0">
                    <div className="font-semibold text-white">
                      {disk.name}
                    </div>

                    <div className="truncate text-xs text-slate-500">
                      {disk.model || "Model unavailable"}
                    </div>
                  </div>
                </div>

                {/* DISK DETAILS */}
                <div className="grid grid-cols-2 gap-x-6 gap-y-2 text-sm sm:grid-cols-4">
                  <div>
                    <div className="text-[10px] uppercase tracking-wide text-slate-600">
                      Capacity
                    </div>

                    <div className="text-slate-200">
                      {formatBytes(disk.capacityBytes)}
                    </div>
                  </div>

                  <div>
                    <div className="text-[10px] uppercase tracking-wide text-slate-600">
                      Status
                    </div>

                    <div className="text-slate-200">
                      {disk.status || "-"}
                    </div>
                  </div>

                  <div>
                    <div className="text-[10px] uppercase tracking-wide text-slate-600">
                      Health
                    </div>

                    <div
                      className={`flex items-center gap-1 ${healthClass(
                        disk.health,
                      )}`}
                    >
                      <HealthIcon health={disk.health} />
                      {disk.health || "-"}
                    </div>
                  </div>

                  <div>
                    <div className="text-[10px] uppercase tracking-wide text-slate-600">
                      Temperature
                    </div>

                    <div className="flex items-center gap-1 text-slate-200">
                      <Thermometer className="h-4 w-4 text-sky-400" />

                      {disk.temperature !== null
                        ? `${disk.temperature} °C`
                        : "-"}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          ))}

          {hardware.disks.length === 0 && (
            <div className="rounded-lg border border-dashed border-slate-700 bg-[#0c1627] p-8 text-center text-sm text-slate-500">
              No disk information available.
            </div>
          )}
        </div>
      </div>
    </section>
  );
}
