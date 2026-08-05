export default function TodayStatisticsCard() {
  const stats = [
    ["Total Login", "519"],
    ["Traffic Download", "13.43 GB"],
    ["Unique Users", "30"],
    ["Traffic Upload", "9.71 GB"],
    ["Average Session", "1h 28m"],
    ["Total Traffic", "23.14 GB"],
  ];

  return (
    <div className="rounded-2xl border border-slate-800 bg-[#111b2d] p-5">
      <h3 className="mb-4 text-xl font-semibold">
        Today Statistics
      </h3>

      <div className="grid grid-cols-2 gap-4">
        {stats.map(([label, value]) => (
          <div
            key={label}
            className="border-b border-slate-800 pb-3"
          >
            <div className="text-sm text-slate-400">
              {label}
            </div>

            <div className="mt-1 text-lg font-semibold">
              {value}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
