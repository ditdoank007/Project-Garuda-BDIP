"use client";

import {
  CheckCircle2,
  Network,
  Server,
} from "lucide-react";

import type { SynologySystemHealth } from "@/types/dashboard";

function formatUptime(value: string) {
  if (!value) return "-";

  const parts = value.split(":");

  if (parts.length !== 3) {
    return value;
  }

  const hours = Number(parts[0]);

  if (!Number.isFinite(hours)) {
    return value;
  }

  const days = Math.floor(hours / 24);
  const remainingHours = hours % 24;

  return days > 0
    ? `${days}D ${remainingHours}H`
    : `${remainingHours}H`;
}

export default function SynologySystemHealthCard({
  health,
}: {
  health: SynologySystemHealth;
}) {
  return (
    <section className="rounded-xl border border-slate-800/60 bg-[#111b2d] p-5">
      <div className="mb-5 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="rounded-lg bg-cyan-600/20 p-2">
            <Server className="h-5 w-5 text-cyan-400" />
          </div>

          <div>
            <h2 className="text-lg font-semibold text-white">
              SYNOLOGY SYSTEM HEALTH
            </h2>

            <p className="text-xs text-slate-400">
              {health.hostname || "Synology NAS"}
            </p>
          </div>
        </div>

        <div className="flex items-center gap-2 rounded-full bg-emerald-500/15 px-3 py-1 text-xs font-semibold text-emerald-400">
          <CheckCircle2 className="h-4 w-4" />
          {health.healthy ? "HEALTHY" : "CHECK"}
        </div>
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <div className="rounded-lg bg-[#0c1627] p-4">
          <p className="text-xs uppercase tracking-wide text-slate-500">
            Uptime
          </p>

          <p className="mt-2 text-2xl font-bold text-white">
            {formatUptime(health.uptime)}
          </p>
        </div>

        <div className="rounded-lg bg-[#0c1627] p-4">
          <p className="text-xs uppercase tracking-wide text-slate-500">
            Interfaces
          </p>

          <p className="mt-2 text-2xl font-bold text-white">
            {health.interfaces.length}
          </p>
        </div>
      </div>

      <div className="mt-4 space-y-2">
        {health.interfaces.map((item) => (
          <div
            key={item.id}
            className="flex items-center justify-between rounded-lg border border-slate-800 bg-[#0c1627] px-4 py-3"
          >
            <div className="flex items-center gap-3">
              <Network className="h-4 w-4 text-cyan-400" />

              <div>
                <p className="text-sm font-medium text-white">
                  {item.id}
                </p>

                <p className="text-xs text-slate-500">
                  {item.type}
                </p>
              </div>
            </div>

            <span className="font-mono text-sm text-slate-300">
              {item.ip}
            </span>
          </div>
        ))}
      </div>
    </section>
  );
}
