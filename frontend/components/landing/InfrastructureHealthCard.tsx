export interface InfrastructureHealthItem {
  label: string;
  status: string;
  healthy: boolean;
}

interface InfrastructureHealthCardProps {
  items: InfrastructureHealthItem[];
  lastUpdated?: string;
}

export default function InfrastructureHealthCard({
  items,
  lastUpdated,
}: InfrastructureHealthCardProps) {
  return (
    <div className="rounded-2xl border border-white/10 bg-white/[0.03] p-5 shadow-lg">
      <div className="mb-5">
        <h3 className="text-lg font-semibold text-white">
          INFRASTRUCTURE HEALTH
        </h3>

        <p className="mt-1 text-xs text-white/45">
          BDIP infrastructure status
        </p>
      </div>

      <div className="space-y-3">
        {items.map((item) => (
          <div
            key={item.label}
            className="flex items-center justify-between"
          >
            <span className="text-xs font-medium tracking-wide text-white/55">
              {item.label}
            </span>

            <div className="flex items-center gap-2">
              <span
                className={`h-2 w-2 rounded-full ${
                  item.healthy
                    ? "bg-emerald-400"
                    : "bg-red-400"
                }`}
              />

              <span
                className={`text-xs font-semibold ${
                  item.healthy
                    ? "text-emerald-400"
                    : "text-red-400"
                }`}
              >
                {item.status}
              </span>
            </div>
          </div>
        ))}
      </div>

      {lastUpdated && (
        <div className="mt-5 border-t border-white/10 pt-4 text-xs text-white/35">
          Last update: {lastUpdated}
        </div>
      )}
    </div>
  );
}
