"use client";

import {
  ResponsiveContainer,
  AreaChart,
  Area,
  CartesianGrid,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";

import type { SynologyConnectionActivity } from "@/types/dashboard";

interface Props {
  connections: SynologyConnectionActivity[];
}

function formatTime(value: string) {
  if (!value) return "-";

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleTimeString("id-ID", {
    hour: "2-digit",
    minute: "2-digit",
  });
}

export default function ActiveSessionsChart({ connections }: Props) {
  const safeConnections = Array.isArray(connections)
    ? connections
    : [];

  const grouped = new Map<string, number>();

  for (const connection of safeConnections) {
    if (!connection.currentConnected) {
      continue;
    }

    const label = formatTime(connection.time);

    grouped.set(
      label,
      (grouped.get(label) ?? 0) + 1,
    );
  }

  const data = Array.from(grouped.entries())
    .map(([time, value]) => ({
      time,
      value,
    }))
    .slice(-24);

  return (
    <div className="rounded-xl border border-slate-800/40 bg-[#111b2d] p-5">

      <div className="mb-5 flex items-center justify-between">

        <div>
          <h2 className="text-lg font-semibold text-white">
            Active Sessions Over Time
          </h2>

          <p className="mt-1 text-xs text-slate-400">
            Synology current connected sessions
          </p>
        </div>

        <div className="rounded bg-green-900/40 px-3 py-1 text-xs text-green-300">
          {safeConnections.filter(
            (connection) => connection.currentConnected,
          ).length} Active
        </div>

      </div>

      {data.length === 0 ? (
        <div className="flex h-[300px] items-center justify-center text-sm text-slate-500">
          No active Synology sessions available
        </div>
      ) : (
        <div className="h-[300px]">

          <ResponsiveContainer width="100%" height="100%">

            <AreaChart data={data}>

              <defs>

                <linearGradient
                  id="synologySessionFill"
                  x1="0"
                  y1="0"
                  x2="0"
                  y2="1"
                >

                  <stop
                    offset="0%"
                    stopColor="#22c55e"
                    stopOpacity={0.5}
                  />

                  <stop
                    offset="100%"
                    stopColor="#22c55e"
                    stopOpacity={0}
                  />

                </linearGradient>

              </defs>

              <CartesianGrid
                stroke="#263447"
                strokeDasharray="4 4"
              />

              <XAxis
                dataKey="time"
                stroke="#64748b"
              />

              <YAxis
                stroke="#64748b"
                allowDecimals={false}
              />

              <Tooltip
                contentStyle={{
                  background: "#0f172a",
                  border: "1px solid #334155",
                  borderRadius: "10px",
                }}
                labelStyle={{
                  color: "#cbd5e1",
                }}
              />

              <Area
                type="monotone"
                dataKey="value"
                name="Active Sessions"
                stroke="#22c55e"
                strokeWidth={3}
                fill="url(#synologySessionFill)"
              />

            </AreaChart>

          </ResponsiveContainer>

        </div>
      )}

    </div>
  );
}
