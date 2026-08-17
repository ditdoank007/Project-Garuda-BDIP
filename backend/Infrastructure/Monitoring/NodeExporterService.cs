using System.Globalization;
using Microsoft.Extensions.Options;

namespace BDIP.Infrastructure.Monitoring;

public sealed class NodeExporterService : INodeExporterService
{
    private readonly HttpClient _httpClient;
    private readonly NodeExporterOptions _options;
    private readonly object _snapshotLock = new();
    private readonly Dictionary<string, MetricSnapshot> _snapshots = new();

    public NodeExporterService(
        HttpClient httpClient,
        IOptions<NodeExporterOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<NodeExporterServerMetrics>> GetServerMetricsAsync(
        CancellationToken cancellationToken = default)
    {
        var targets = new[]
        {
            _options.Server,
            _options.Database
        };

        var results = new List<NodeExporterServerMetrics>(targets.Length);

        foreach (var target in targets)
        {
            results.Add(
                await CollectServerMetricsAsync(
                    target,
                    cancellationToken));
        }

        return results;
    }

    private async Task<NodeExporterServerMetrics> CollectServerMetricsAsync(
        NodeExporterOptions.ServerOptions target,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        try
        {
            var client = _httpClient;

            using var response = await client.GetAsync(
                $"{target.BaseUrl.TrimEnd('/')}/metrics",
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadAsStringAsync(
                cancellationToken);

            var metrics = ParseMetrics(payload);

            var currentCpuTotal = GetCpuTotal(metrics);
            var currentCpuIdle = GetCpuIdle(metrics);

            var currentNetworkReceive = GetNetworkValue(
                metrics,
                "node_network_receive_bytes_total");

            var currentNetworkTransmit = GetNetworkValue(
                metrics,
                "node_network_transmit_bytes_total");

            double? cpuPercent = null;
            double? receiveRate = null;
            double? transmitRate = null;

            lock (_snapshotLock)
            {
                if (_snapshots.TryGetValue(
                        target.Name,
                        out var previous))
                {
                    var elapsedSeconds =
                        (now - previous.Timestamp).TotalSeconds;

                    if (elapsedSeconds > 0)
                    {
                        var cpuTotalDelta =
                            currentCpuTotal - previous.CpuTotal;

                        var cpuIdleDelta =
                            currentCpuIdle - previous.CpuIdle;

                        if (cpuTotalDelta > 0)
                        {
                            cpuPercent = Math.Round(
                                Math.Clamp(
                                    (1 - (cpuIdleDelta / cpuTotalDelta)) * 100,
                                    0,
                                    100),
                                2);
                        }

                        receiveRate = Math.Round(
                            Math.Max(
                                0,
                                (currentNetworkReceive -
                                    previous.NetworkReceive) /
                                elapsedSeconds),
                            2);

                        transmitRate = Math.Round(
                            Math.Max(
                                0,
                                (currentNetworkTransmit -
                                    previous.NetworkTransmit) /
                                elapsedSeconds),
                            2);
                    }
                }

                _snapshots[target.Name] = new MetricSnapshot(
                    currentCpuTotal,
                    currentCpuIdle,
                    currentNetworkReceive,
                    currentNetworkTransmit,
                    now);
            }

            var memoryTotal = GetValue(
                metrics,
                "node_memory_MemTotal_bytes");

            var memoryAvailable = GetValue(
                metrics,
                "node_memory_MemAvailable_bytes");

            var swapTotal = GetValue(
                metrics,
                "node_memory_SwapTotal_bytes");

            var swapFree = GetValue(
                metrics,
                "node_memory_SwapFree_bytes");

            var diskTotal = GetFilesystemValue(
                metrics,
                "node_filesystem_size_bytes");

            var diskAvailable = GetFilesystemValue(
                metrics,
                "node_filesystem_avail_bytes");

            var bootTime = GetValue(
                metrics,
                "node_boot_time_seconds");

            var memoryPercent = memoryTotal > 0
                ? ((memoryTotal - memoryAvailable) / memoryTotal) * 100
                : 0;

            var swapPercent = swapTotal > 0
                ? ((swapTotal - swapFree) / swapTotal) * 100
                : 0;

            var diskPercent = diskTotal > 0
                ? ((diskTotal - diskAvailable) / diskTotal) * 100
                : 0;

            var uptimeSeconds = bootTime > 0
                ? Math.Max(
                    0,
                    (long)(
                        DateTimeOffset.UtcNow -
                        DateTimeOffset.FromUnixTimeSeconds(
                            (long)bootTime))
                    .TotalSeconds)
                : 0;

            return new NodeExporterServerMetrics
            {
                Name = target.Name,
                IsOnline = true,
                CpuPercent = cpuPercent,

                MemoryTotalBytes = ToLong(memoryTotal),
                MemoryAvailableBytes = ToLong(memoryAvailable),
                MemoryPercent = Math.Round(memoryPercent, 2),

                SwapTotalBytes = ToLong(swapTotal),
                SwapFreeBytes = ToLong(swapFree),
                SwapPercent = Math.Round(swapPercent, 2),

                DiskTotalBytes = ToLong(diskTotal),
                DiskAvailableBytes = ToLong(diskAvailable),
                DiskPercent = Math.Round(diskPercent, 2),

                NetworkReceiveBytesPerSecond = receiveRate,
                NetworkTransmitBytesPerSecond = transmitRate,

                UptimeSeconds = uptimeSeconds,
                LastUpdated = now
            };
        }
        catch
        {
            return new NodeExporterServerMetrics
            {
                Name = target.Name,
                IsOnline = false,
                LastUpdated = now
            };
        }
    }

    private static double GetCpuTotal(
        Dictionary<string, double> metrics)
    {
        return metrics
            .Where(x =>
                x.Key.StartsWith(
                    "node_cpu_seconds_total{",
                    StringComparison.Ordinal))
            .Sum(x => x.Value);
    }

    private static double GetCpuIdle(
        Dictionary<string, double> metrics)
    {
        return metrics
            .Where(x =>
                x.Key.StartsWith(
                    "node_cpu_seconds_total{",
                    StringComparison.Ordinal) &&
                x.Key.Contains(
                    "mode=\"idle\"",
                    StringComparison.Ordinal))
            .Sum(x => x.Value);
    }

    private static Dictionary<string, double> ParseMetrics(
        string payload)
    {
        var result = new Dictionary<string, double>(
            StringComparer.Ordinal);

        using var reader = new StringReader(payload);

        string? line;

        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line) ||
                line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            var separator = line.LastIndexOf(' ');

            if (separator <= 0 ||
                separator >= line.Length - 1)
            {
                continue;
            }

            var metricName = line[..separator];
            var valueText = line[(separator + 1)..];

            if (double.TryParse(
                    valueText,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var value))
            {
                result[metricName] = value;
            }
        }

        return result;
    }

    private static double GetValue(
        Dictionary<string, double> metrics,
        string name)
    {
        return metrics.TryGetValue(
            name,
            out var value)
            ? value
            : 0;
    }

    private static double GetFilesystemValue(
        Dictionary<string, double> metrics,
        string name)
    {
        var prefix = $"{name}{{";

        var candidate = metrics
            .Where(x =>
                x.Key.StartsWith(
                    prefix,
                    StringComparison.Ordinal))
            .FirstOrDefault(x =>
                x.Key.Contains(
                    "mountpoint=\"/\"",
                    StringComparison.Ordinal));

        return candidate.Equals(
            default(KeyValuePair<string, double>))
            ? 0
            : candidate.Value;
    }

    private static double GetNetworkValue(
        Dictionary<string, double> metrics,
        string name)
    {
        var prefix = $"{name}{{";

        var candidate = metrics
            .Where(x =>
                x.Key.StartsWith(
                    prefix,
                    StringComparison.Ordinal))
            .FirstOrDefault(x =>
                x.Key.Contains(
                    "device=\"eth0\"",
                    StringComparison.Ordinal));

        return candidate.Equals(
            default(KeyValuePair<string, double>))
            ? 0
            : candidate.Value;
    }

    private static long ToLong(double value)
    {
        return value <= 0
            ? 0
            : Convert.ToInt64(value);
    }

    private sealed record MetricSnapshot(
        double CpuTotal,
        double CpuIdle,
        double NetworkReceive,
        double NetworkTransmit,
        DateTimeOffset Timestamp);
}
