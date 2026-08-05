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

const data = [
  { time: "00:00", value: 6 },
  { time: "02:00", value: 8 },
  { time: "04:00", value: 12 },
  { time: "06:00", value: 18 },
  { time: "08:00", value: 42 },
  { time: "09:00", value: 38 },
  { time: "10:00", value: 54 },
  { time: "12:00", value: 36 },
  { time: "14:00", value: 30 },
  { time: "16:00", value: 45 },
  { time: "18:00", value: 48 },
  { time: "20:00", value: 25 },
  { time: "22:00", value: 33 },
];

export default function ActiveSessionsChart() {
  return (
    <div className="rounded-xl border border-slate-800/40 bg-[#111b2d] p-5">

      <div className="mb-5 flex items-center justify-between">

        <h2 className="text-lg font-semibold text-white">
          Active Sessions Over Time
        </h2>

        <div className="flex gap-2">

          <button className="rounded bg-blue-700 px-3 py-1 text-xs">
            Today
          </button>

          <button className="rounded bg-slate-800 px-3 py-1 text-xs text-slate-300">
            7 Days
          </button>

          <button className="rounded bg-slate-800 px-3 py-1 text-xs text-slate-300">
            30 Days
          </button>

        </div>

      </div>

      <div className="h-[300px]">

        <ResponsiveContainer width="100%" height="100%">

          <AreaChart data={data}>

            <defs>

              <linearGradient id="sessionFill" x1="0" y1="0" x2="0" y2="1">

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
            />

            <Tooltip
              contentStyle={{
                background: "#0f172a",
                border: "1px solid #334155",
                borderRadius: "10px",
              }}
            />

            <Area
              type="monotone"
              dataKey="value"
              stroke="#22c55e"
              strokeWidth={3}
              fill="url(#sessionFill)"
            />

          </AreaChart>

        </ResponsiveContainer>

      </div>

    </div>
  );
}
