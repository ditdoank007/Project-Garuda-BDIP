"use client";

const nas = [
  { name: "vlan11-SAR.SURABAYA", online: true },
  { name: "vlan10-SAR.TRENGGALEK", online: true },
  { name: "vlan20-SAR.SUMENEP", online: true },
  { name: "vlan30-SAR.MALANG", online: true },
  { name: "vlan21-SAR.LAMONGAN", online: false },
  { name: "vlan22-SAR.BOJONEGORO", online: false },
];

export default function NasStatusCard() {
  return (
    <div className="rounded-2xl border border-slate-800 bg-[#111b2d] p-6 shadow-lg">

      <h3 className="mb-6 text-xl font-semibold text-white">
        NAS Status
      </h3>

      <div className="space-y-4">

        {nas.map((item) => (

          <div
            key={item.name}
            className="flex items-center justify-between"
          >

            <div className="flex items-center gap-3">

              <span
                className={`h-3 w-3 rounded-full ${
                  item.online
                    ? "bg-green-500"
                    : "bg-red-500"
                }`}
              />

              <span className="text-sm text-slate-200">
                {item.name}
              </span>

            </div>

            <span
              className={`text-sm font-medium ${
                item.online
                  ? "text-green-400"
                  : "text-red-400"
              }`}
            >
              {item.online ? "Online" : "Offline"}
            </span>

          </div>

        ))}

      </div>

    </div>
  );
}
