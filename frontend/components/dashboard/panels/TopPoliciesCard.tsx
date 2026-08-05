"use client";

const data = [
  { name: "ASN", value: 1200, color: "#22c55e" },
  { name: "PEGAWAI", value: 620, color: "#4ade80" },
  { name: "ADMIN", value: 145, color: "#84cc16" },
  { name: "VPN-ACCESS", value: 99, color: "#a3e635" },
];

const max = Math.max(...data.map((x) => x.value));

export default function TopPoliciesCard() {
  return (
    <div className="rounded-2xl border border-slate-800 bg-[#111b2d] p-6 shadow-lg">

      <h3 className="mb-6 text-xl font-semibold text-white">
        Top Policies
      </h3>

      <div className="space-y-5">

        {data.map((item) => (

          <div key={item.name}>

            <div className="mb-2 flex items-center justify-between">

              <span className="text-sm text-slate-200">
                {item.name}
              </span>

              <span className="text-sm text-slate-400">
                {new Intl.NumberFormat("id-ID").format(item.value)}
              </span>

            </div>

            <div className="h-2 overflow-hidden rounded-full bg-slate-700">

              <div
                className="h-full rounded-full transition-all duration-700"
                style={{
                  width: `${(item.value / max) * 100}%`,
                  backgroundColor: item.color,
                }}
              />

            </div>

          </div>

        ))}

      </div>

    </div>
  );
}
