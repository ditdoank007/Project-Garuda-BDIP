import type { MonitoringServer } from "@/types/monitoring";

interface ServerMonitoringCardProps {
  server: MonitoringServer;
  title: string;
  subtitle: string;
}

function formatGiB(bytes: number): string {
  return `${(bytes / 1024 ** 3).toFixed(2)} GiB`;
}

function formatMiB(bytes: number): string {
  return `${(bytes / 1024 ** 2).toFixed(0)} MiB`;
}

function formatUsed(total: number, available: number): string {
  const used = Math.max(0, total - available);

  if (total < 1024 ** 3) {
    return `${formatMiB(used)} / ${formatMiB(total)}`;
  }

  return `${formatGiB(used)} / ${formatGiB(total)}`;
}

function formatPercent(value: number | null): string {
  return value === null ? "--" : `${value.toFixed(2)}%`;
}

export default function ServerMonitoringCard({
  server,
  title,
  subtitle,
}: ServerMonitoringCardProps) {
  const healthy = server.isOnline;

  return (
    <div className="rounded-2xl border border-white/10 bg-white/[0.03] p-5 shadow-lg">
      <div className="mb-5 flex items-start justify-between">
        <div>
          <h3 className="text-lg font-semibold text-white">
            {title}
          </h3>

          <p className="mt-1 text-xs text-white/45">
            {subtitle}
          </p>
        </div>

        <div className="flex items-center gap-2 text-xs font-medium">
          <span
            className={`h-2 w-2 rounded-full ${
              healthy ? "bg-emerald-400" : "bg-red-400"
            }`}
          />

          <span
            className={
              healthy ? "text-emerald-400" : "text-red-400"
            }
          >
            {healthy ? "HEALTHY" : "OFFLINE"}
          </span>
        </div>
      </div>

      <div className="space-y-3">
        <MetricRow
          label="CPU"
          value={formatPercent(server.cpuPercent)}
        />

        <MetricRow
          label="MEMORY"
          value={`${server.memoryPercent.toFixed(2)}%`}
        />

        <MetricRow
          label="SWAP"
          value={`${server.swapPercent.toFixed(2)}%`}
        />

        <MetricRow
          label="STORAGE"
          value={`${server.diskPercent.toFixed(2)}%`}
        />
      </div>

      <div className="mt-5 space-y-1 border-t border-white/10 pt-4 text-xs text-white/45">
        <div>{formatUsed(server.memoryTotalBytes, server.memoryAvailableBytes)} RAM</div>
        <div>{formatUsed(server.swapTotalBytes, server.swapFreeBytes)} SWAP</div>
        <div>{formatUsed(server.diskTotalBytes, server.diskAvailableBytes)} STORAGE</div>
      </div>
    </div>
  );
}

interface MetricRowProps {
  label: string;
  value: string;
}

function MetricRow({
  label,
  value,
}: MetricRowProps) {
  return (
    <div className="flex items-center justify-between">
      <span className="text-xs font-medium tracking-wide text-white/45">
        {label}
      </span>

      <span className="font-mono text-sm font-semibold text-white">
        {value}
      </span>
    </div>
  );
}
