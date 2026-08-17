using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using Lextm.SharpSnmpLib.Security;
using Microsoft.Extensions.Options;
using System.Net;

namespace BDIP.Infrastructure.Synology;

public sealed class SynologySnmpService
{
    private readonly SynologySnmpOptions _options;

    private const string SystemRoot =
        "1.3.6.1.4.1.6574.1";

    private const string StorageRoot =
        "1.3.6.1.4.1.6574.3";

    private const string CpuOid =
        "1.3.6.1.4.1.6574.1.7.1.0";

    private const string MemoryOid =
        "1.3.6.1.4.1.6574.1.7.2.0";

    private const string TemperatureOid =
        "1.3.6.1.4.1.6574.1.2.0";

    private const string SystemFanOid =
        "1.3.6.1.4.1.6574.1.4.1.0";

    private const string CpuFanOid =
        "1.3.6.1.4.1.6574.1.4.2.0";

    private const string ReadBytesOid =
        "1.3.6.1.4.1.6574.102.1.1.12.1";

    private const string WriteBytesOid =
        "1.3.6.1.4.1.6574.102.1.1.13.1";

    private const string ReadOperationsOid =
        "1.3.6.1.4.1.6574.102.1.1.5.1";

    private const string WriteOperationsOid =
        "1.3.6.1.4.1.6574.102.1.1.6.1";

    private static readonly object PerformanceLock = new();

    private static PerformanceBaseline? _performanceBaseline;

    public SynologySnmpService(
        IOptions<SynologySnmpOptions> options)
    {
        _options = options.Value;
    }

    public async Task<SynologySnmpSnapshot> GetSnapshotAsync(
        string host,
        CancellationToken cancellationToken = default)
    {
        var system = await GetSystemAsync(
            host,
            cancellationToken);

        var storage = await GetStorageAsync(
            host,
            cancellationToken);

        var resources = await GetSystemResourcesAsync(
            host,
            cancellationToken);

        var performance = await GetPerformanceAsync(
            host,
            cancellationToken);

        return new SynologySnmpSnapshot
        {
            Online = true,

            Model = system.Model,
            Serial = system.Serial,
            DsmVersion = system.DsmVersion,

            SystemStatus = system.SystemStatus,

            VolumeName = storage.VolumeName,
            TotalBytes = storage.TotalBytes,
            UsedBytes = storage.UsedBytes,
            FreeBytes = storage.FreeBytes,
            UsagePercent = storage.UsagePercent,
            VolumeStatus = storage.VolumeStatus,

            CpuPercent = resources.CpuPercent,
            MemoryPercent = resources.MemoryPercent,
            TemperatureC = resources.TemperatureC,
            SystemFanStatus = resources.SystemFanStatus,
            CpuFanStatus = resources.CpuFanStatus,

            ReadBytesPerSecond = performance.ReadBytesPerSecond,
            WriteBytesPerSecond = performance.WriteBytesPerSecond,
            ReadIops = performance.ReadIops,
            WriteIops = performance.WriteIops
        };
    }

    private async Task<SynologyPerformanceData> GetPerformanceAsync(
        string host,
        CancellationToken cancellationToken)
    {
        var variables = new List<Variable>
        {
            new(
                new ObjectIdentifier(
                    ReadBytesOid)),

            new(
                new ObjectIdentifier(
                    WriteBytesOid)),

            new(
                new ObjectIdentifier(
                    ReadOperationsOid)),

            new(
                new ObjectIdentifier(
                    WriteOperationsOid))
        };

        var response = await GetAsync(
            host,
            variables,
            cancellationToken);

        var current = new PerformanceBaseline
        {
            Timestamp = DateTime.UtcNow,
            ReadBytes = GetCounter64(response[0]),
            WriteBytes = GetCounter64(response[1]),
            ReadOperations = GetCounter64(response[2]),
            WriteOperations = GetCounter64(response[3])
        };

        lock (PerformanceLock)
        {
            if (_performanceBaseline is null)
            {
                _performanceBaseline = current;

                return new SynologyPerformanceData();
            }

            var previous = _performanceBaseline;
            _performanceBaseline = current;

            var elapsedSeconds =
                (current.Timestamp - previous.Timestamp)
                    .TotalSeconds;

            if (elapsedSeconds <= 0)
            {
                return new SynologyPerformanceData();
            }

            var readBytesDelta =
                Math.Max(
                    0,
                    current.ReadBytes - previous.ReadBytes);

            var writeBytesDelta =
                Math.Max(
                    0,
                    current.WriteBytes - previous.WriteBytes);

            var readOperationsDelta =
                Math.Max(
                    0,
                    current.ReadOperations - previous.ReadOperations);

            var writeOperationsDelta =
                Math.Max(
                    0,
                    current.WriteOperations - previous.WriteOperations);

            return new SynologyPerformanceData
            {
                ReadBytesPerSecond =
                    readBytesDelta / elapsedSeconds,

                WriteBytesPerSecond =
                    writeBytesDelta / elapsedSeconds,

                ReadIops =
                    readOperationsDelta / elapsedSeconds,

                WriteIops =
                    writeOperationsDelta / elapsedSeconds
            };
        }
    }

    private async Task<SynologySystemData> GetSystemAsync(
        string host,
        CancellationToken cancellationToken)
    {
        var variables = new List<Variable>
        {
            new(
                new ObjectIdentifier(
                    $"{SystemRoot}.1.0")),

            new(
                new ObjectIdentifier(
                    $"{SystemRoot}.2.0")),

            new(
                new ObjectIdentifier(
                    $"{SystemRoot}.5.1.0")),

            new(
                new ObjectIdentifier(
                    $"{SystemRoot}.5.2.0")),

            new(
                new ObjectIdentifier(
                    $"{SystemRoot}.5.3.0")),

            new(
                new ObjectIdentifier(
                    $"{SystemRoot}.5.4.0"))
        };

        var response = await GetAsync(
            host,
            variables,
            cancellationToken);

        return new SynologySystemData
        {
            SystemStatus =
                GetInteger(response[0]),

            Model =
                GetString(response[2]),

            Serial =
                GetString(response[3]),

            DsmVersion =
                GetString(response[4])
        };
    }

    private async Task<SynologySystemResourcesData> GetSystemResourcesAsync(
        string host,
        CancellationToken cancellationToken)
    {
        var variables = new List<Variable>
        {
            new(
                new ObjectIdentifier(
                    CpuOid)),

            new(
                new ObjectIdentifier(
                    MemoryOid)),

            new(
                new ObjectIdentifier(
                    TemperatureOid)),

            new(
                new ObjectIdentifier(
                    SystemFanOid)),

            new(
                new ObjectIdentifier(
                    CpuFanOid))
        };

        var response = await GetAsync(
            host,
            variables,
            cancellationToken);

        return new SynologySystemResourcesData
        {
            CpuPercent = GetInteger(response[0]),
            MemoryPercent = GetInteger(response[1]),
            TemperatureC = GetInteger(response[2]),
            SystemFanStatus = GetInteger(response[3]),
            CpuFanStatus = GetInteger(response[4])
        };
    }

    private async Task<SynologyStorageData> GetStorageAsync(
        string host,
        CancellationToken cancellationToken)
    {
        var variables = new List<Variable>
        {
            new(
                new ObjectIdentifier(
                    $"{StorageRoot}.1.1.2.0")),

            new(
                new ObjectIdentifier(
                    $"{StorageRoot}.1.1.3.0")),

            new(
                new ObjectIdentifier(
                    $"{StorageRoot}.1.1.4.0")),

            new(
                new ObjectIdentifier(
                    $"{StorageRoot}.1.1.5.0"))
        };

        var response = await GetAsync(
            host,
            variables,
            cancellationToken);

        var status =
            GetInteger(response[1]);

        var free =
            GetCounter64(response[2]);

        var total =
            GetCounter64(response[3]);

        var used =
            Math.Max(
                0,
                total - free);

        var usage =
            total > 0
                ? used * 100.0 / total
                : 0;

        return new SynologyStorageData
        {
            VolumeName =
                GetString(response[0]),

            UsedBytes =
                used,

            TotalBytes =
                total,

            FreeBytes =
                free,

            UsagePercent =
                usage,

            VolumeStatus =
                status
        };
    }

    private async Task<IList<Variable>> GetAsync(
        string host,
        IList<Variable> variables,
        CancellationToken cancellationToken)
    {
        var endpoint =
            new IPEndPoint(
                IPAddress.Parse(host),
                161);

        var authentication =
            new SHA1AuthenticationProvider(
                new OctetString(
                    _options.AuthPassword));

        var privacy =
            new AESPrivacyProvider(
                new OctetString(
                    _options.PrivPassword),
                authentication);

        var username =
            new OctetString(
                _options.Username);

        var registry =
            new UserRegistry(
                new[]
                {
                    new User(
                        username,
                        privacy)
                });

        var discovery =
            Messenger.GetNextDiscovery(
                SnmpType.GetRequestPdu);

        var report =
            await discovery.GetResponseAsync(
                endpoint);

        var request =
            new GetRequestMessage(
                VersionCode.V3,
                Messenger.NextMessageId,
                Messenger.NextRequestId,
                username,
                variables,
                privacy,
                Messenger.MaxMessageSize,
                report);

        var response =
            await request.GetResponseAsync(
                endpoint,
                registry,
                cancellationToken);

        return response
            .Pdu()
            .Variables;
    }

    private static string GetString(
        Variable variable)
    {
        return variable.Data.ToString();
    }

    private static long GetInteger(
        Variable variable)
    {
        return Convert.ToInt64(
            variable.Data.ToString());
    }

    private static long GetCounter64(
        Variable variable)
    {
        return Convert.ToInt64(
            variable.Data.ToString());
    }

    private sealed class SynologySystemData
    {
        public long SystemStatus { get; init; }

        public string Model { get; init; } = "";

        public string Serial { get; init; } = "";

        public string DsmVersion { get; init; } = "";
    }

    private sealed class SynologyStorageData
    {
        public string VolumeName { get; init; } = "";

        public long UsedBytes { get; init; }

        public long TotalBytes { get; init; }

        public long FreeBytes { get; init; }

        public double UsagePercent { get; init; }

        public long VolumeStatus { get; init; }
    }
}

public sealed class SynologyPerformanceData
{
    public double? ReadBytesPerSecond { get; init; }

    public double? WriteBytesPerSecond { get; init; }

    public double? ReadIops { get; init; }

    public double? WriteIops { get; init; }
}

public sealed class PerformanceBaseline
{
    public DateTime Timestamp { get; init; }

    public long ReadBytes { get; init; }

    public long WriteBytes { get; init; }

    public long ReadOperations { get; init; }

    public long WriteOperations { get; init; }
}

public sealed class SynologySystemResourcesData
{
    public long CpuPercent { get; init; }

    public long MemoryPercent { get; init; }

    public long TemperatureC { get; init; }

    public long SystemFanStatus { get; init; }

    public long CpuFanStatus { get; init; }
}

public sealed class SynologySnmpSnapshot
{
    public bool Online { get; init; }

    public string Model { get; init; } = "";

    public double? ReadBytesPerSecond { get; init; }

    public double? WriteBytesPerSecond { get; init; }

    public double? ReadIops { get; init; }

    public double? WriteIops { get; init; }

    public long CpuPercent { get; init; }

    public long MemoryPercent { get; init; }

    public long TemperatureC { get; init; }

    public long SystemFanStatus { get; init; }

    public long CpuFanStatus { get; init; }

    public string Serial { get; init; } = "";

    public string DsmVersion { get; init; } = "";

    public long SystemStatus { get; init; }

    public string VolumeName { get; init; } = "";

    public long TotalBytes { get; init; }

    public long UsedBytes { get; init; }

    public long FreeBytes { get; init; }

    public double UsagePercent { get; init; }

    public long VolumeStatus { get; init; }
}
