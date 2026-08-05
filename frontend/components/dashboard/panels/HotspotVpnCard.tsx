"use client";

import {
  PieChart,
  Pie,
  Cell,
  ResponsiveContainer,
} from "recharts";

const data = [
  {
    name: "Hotspot",
    value: 18,
    color: "#f59e0b",
  },
  {
    name: "OVPN / PPP",
    value: 13,
    color: "#a855f7",
  },
];

const total = data.reduce((a, b) => a + b.value, 0);

export default function HotspotVpnCard() {
  return (
    <div className="rounded-2xl border border-slate-800 bg-[#111b2d] p-6 shadow-lg">

      <h3 className="mb-5 text-xl font-semibold text-white">
        Hotspot vs OVPN
      </h3>

      <div className="h-48">

        <ResponsiveContainer width="100%" height="100%">

          <PieChart>

            <Pie
              data={data}
              innerRadius={55}
              outerRadius={75}
              dataKey="value"
              strokeWidth={0}
            >
              {data.map((item) => (
                <Cell
                  key={item.name}
                  fill={item.color}
                />
              ))}
            </Pie>

          </PieChart>

        </ResponsiveContainer>

      </div>

      <div className="mt-3 grid grid-cols-2 gap-4">

        {data.map((item) => (
          <div key={item.name}>

            <div
              className="text-sm font-semibold"
              style={{
                color: item.color,
              }}
            >
              {item.name}
            </div>

            <div className="mt-1 text-2xl font-bold text-white">
              {Math.round(item.value / total * 100)}%
            </div>

            <div className="text-sm text-slate-400">
              {item.value} Sessions
            </div>

          </div>
        ))}

      </div>

    </div>
  );
}
