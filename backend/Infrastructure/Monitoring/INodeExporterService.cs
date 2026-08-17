namespace BDIP.Infrastructure.Monitoring;

public interface INodeExporterService
{
    Task<IReadOnlyList<NodeExporterServerMetrics>> GetServerMetricsAsync(
        CancellationToken cancellationToken = default);
}

public sealed class NodeExporterServerMetrics
{
    public string Name { get; init; } = string.Empty;
    public bool IsOnline { get; init; }

    public double? CpuPercent { get; init; }

    public long MemoryTotalBytes { get; init; }
    public long MemoryAvailableBytes { get; init; }
    public double MemoryPercent { get; init; }

    public long SwapTotalBytes { get; init; }
    public long SwapFreeBytes { get; init; }
    public double SwapPercent { get; init; }

    public long DiskTotalBytes { get; init; }
    public long DiskAvailableBytes { get; init; }
    public double DiskPercent { get; init; }

    public double? NetworkReceiveBytesPerSecond { get; init; }
    public double? NetworkTransmitBytesPerSecond { get; init; }

    public long UptimeSeconds { get; init; }

    public DateTimeOffset LastUpdated { get; init; }
}
