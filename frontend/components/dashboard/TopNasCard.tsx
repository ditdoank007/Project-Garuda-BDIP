"use client";

const data = [
  { name: "vlan11-SAR.SURABAYA", value: 238 },
  { name: "vlan10-KANTOR.PUSAT", value: 183 },
  { name: "vlan20-SAR.MAKASSAR", value: 141 },
  { name: "vlan30-SAR.AMBON", value: 102 },
  { name: "vlan21-SAR.MEDAN", value: 88 },
];

const max = Math.max(...data.map((d) => d.value));

export default function TopNasCard() {
  return (
    <div className="rounded-xl border border-slate-800/40 bg-[#111b2d] p-5">

      <h2 className="mb-5 text-lg font-semibold text-white">
        Top NAS
      </h2>

      <div className="space-y-4">

        {data.map((item, index) => (

          <div key={item.name}>

            <div className="mb-1 flex justify-between text-sm">

              <span className="text-slate-300">
                {index + 1}. {item.name}
              </span>

              <span className="text-slate-400">
                {item.value}
              </span>

            </div>

            <div className="h-2 rounded-full bg-slate-800">

              <div
                className="h-2 rounded-full bg-sky-500 transition-all"
                style={{
                  width: `${(item.value / max) * 100}%`,
                }}
              />

            </div>

          </div>

        ))}

      </div>

    </div>
  );
}
