export default function TerminateCauseCard() {
  const data = [
    { label: "User Logout", value: 52, color: "bg-green-500" },
    { label: "Session Timeout", value: 38, color: "bg-yellow-500" },
    { label: "Idle Timeout", value: 21, color: "bg-red-500" },
    { label: "Lost Carrier", value: 9, color: "bg-purple-500" },
    { label: "Admin Reset", value: 4, color: "bg-blue-500" },
  ];

  const total = data.reduce((a, b) => a + b.value, 0);

  return (
    <div className="rounded-2xl border border-slate-800 bg-[#111b2d] p-5">
      <h3 className="mb-4 text-xl font-semibold">
        Session Terminate Cause
      </h3>

      <div className="space-y-3">
        {data.map((item) => (
          <div key={item.label}>
            <div className="mb-1 flex justify-between text-sm">
              <span>{item.label}</span>
              <span>
                {item.value} ({Math.round(item.value / total * 100)}%)
              </span>
            </div>

            <div className="h-2 rounded bg-slate-700">
              <div
                className={`${item.color} h-2 rounded`}
                style={{
                  width: `${(item.value / total) * 100}%`,
                }}
              />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
