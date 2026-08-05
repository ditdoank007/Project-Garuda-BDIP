type NasItem = {
  name: string;
  value: number;
};

const data: NasItem[] = [
  { name: "vlan11-SAR.SURABAYA", value: 238 },
  { name: "vlan10-SAR.TRENGGALEK", value: 183 },
  { name: "vlan20-SAR.SUMENEP", value: 141 },
  { name: "vlan30-SAR.MALANG", value: 102 },
  { name: "vlan21-SAR.LAMONGAN", value: 88 },
  { name: "vlan22-SAR.BOJONEGORO", value: 80 },
];

const max = Math.max(...data.map((x) => x.value));

export default function TopNasCard() {
  return (
    <div className="rounded-2xl border border-slate-800 bg-[#111b2d] p-6 shadow-lg">
      <h3 className="mb-6 text-xl font-semibold text-white">
        Top NAS
      </h3>

      <div className="space-y-5">
        {data.map((item, index) => (
          <div key={item.name}>
            <div className="mb-2 flex items-center justify-between text-sm">
              <span className="text-slate-200">
                {index + 1}. {item.name}
              </span>

              <span className="font-medium text-slate-400">
                {item.value}
              </span>
            </div>

            <div className="h-2 overflow-hidden rounded-full bg-slate-700">
              <div
                className="h-full rounded-full bg-sky-500 transition-all duration-700"
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
