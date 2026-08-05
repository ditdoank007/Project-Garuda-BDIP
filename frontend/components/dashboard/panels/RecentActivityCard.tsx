export default function RecentActivityCard() {
  const rows = [
    {
      user: "rizka.iriani",
      action: "Login via Hotspot",
      target: "vlan11-SAR.SURABAYA",
      time: "20:13:41",
    },
    {
      user: "dityo.mahendro",
      action: "Login via OVPN",
      target: "10.25.25.115",
      time: "20:12:33",
    },
    {
      user: "agus.setiawan",
      action: "Disconnect",
      target: "Timeout",
      time: "20:11:02",
    },
    {
      user: "budi.santoso",
      action: "Login via Hotspot",
      target: "vlan10-KANTOR.PUSAT",
      time: "20:10:18",
    },
    {
      user: "yohana",
      action: "Login via OVPN",
      target: "10.25.24.18",
      time: "20:09:47",
    },
  ];

  return (
    <div className="rounded-2xl border border-slate-800 bg-[#111b2d] p-5">
      <h3 className="mb-4 text-xl font-semibold">
        Recent Activity
      </h3>

      <div className="space-y-3">
        {rows.map((item) => (
          <div
            key={item.user + item.time}
            className="flex justify-between text-sm border-b border-slate-800 pb-2"
          >
            <div>
              <div className="font-medium">{item.user}</div>
              <div className="text-slate-400">
                {item.action}
              </div>
            </div>

            <div className="text-right">
              <div>{item.target}</div>
              <div className="text-slate-500">
                {item.time}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
