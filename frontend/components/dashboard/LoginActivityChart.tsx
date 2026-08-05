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

const data = [
  { time: "00:00", login: 22 },
  { time: "02:00", login: 30 },
  { time: "04:00", login: 28 },
  { time: "06:00", login: 45 },
  { time: "08:00", login: 80 },
  { time: "09:00", login: 65 },
  { time: "10:00", login: 87 },
  { time: "12:00", login: 52 },
  { time: "14:00", login: 40 },
  { time: "16:00", login: 55 },
  { time: "18:00", login: 48 },
  { time: "20:00", login: 35 },
  { time: "22:00", login: 42 },
];

export default function LoginActivityChart() {
  return (
    <div className="rounded-xl border border-slate-800/40 bg-[#111b2d] p-5">

      <div className="mb-5 flex items-center justify-between">

        <h2 className="text-lg font-semibold text-white">
          Login Activity
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
            />

            <Tooltip
              contentStyle={{
                background: "#0f172a",
                border: "1px solid #334155",
                borderRadius: "10px",
              }}
            />

            <Line
              type="monotone"
              dataKey="login"
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

    </div>
  );
}
