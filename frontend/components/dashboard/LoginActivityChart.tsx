"use client";

import {
  ResponsiveContainer,
  LineChart,
  Line,
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

export default function LoginActivityChart({ connections }: Props) {
  const safeConnections = Array.isArray(connections)
    ? connections
    : [];

  const grouped = new Map<string, number>();

  for (const connection of safeConnections) {
    const label = formatTime(connection.time);

    grouped.set(
      label,
      (grouped.get(label) ?? 0) + 1,
    );
  }

  const data = Array.from(grouped.entries())
    .map(([time, login]) => ({
      time,
      login,
    }))
    .slice(-24);

  return (
    <div className="rounded-xl border border-slate-800/40 bg-[#111b2d] p-5">

      <div className="mb-5 flex items-center justify-between">

        <div>
          <h2 className="text-lg font-semibold text-white">
            Login Activity
          </h2>

          <p className="mt-1 text-xs text-slate-400">
            Synology connection activity
          </p>
        </div>

        <div className="rounded bg-blue-900/40 px-3 py-1 text-xs text-blue-300">
          {safeConnections.length} Connections
        </div>

      </div>

      {data.length === 0 ? (
        <div className="flex h-[300px] items-center justify-center text-sm text-slate-500">
          No Synology login activity available
        </div>
      ) : (
        <div className="h-[300px]">

          <ResponsiveContainer width="100%" height="100%">

            <LineChart data={data}>

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

              <Line
                type="monotone"
                dataKey="login"
                name="Connections"
                stroke="#60a5fa"
                strokeWidth={3}
                dot={false}
                activeDot={{
                  r: 6,
                }}
              />

            </LineChart>

          </ResponsiveContainer>

        </div>
      )}

    </div>
  );
}
