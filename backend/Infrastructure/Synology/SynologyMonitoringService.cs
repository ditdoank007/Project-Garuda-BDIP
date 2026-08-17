using System.Net;
using System.Text.Json;
using BDIP.Application.Synology;
using BDIP.Contracts.Synology;
using Microsoft.Extensions.Configuration;

namespace BDIP.Infrastructure.Synology;

public class SynologyMonitoringService : ISynologyMonitoringService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public SynologyMonitoringService(
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _configuration = configuration;
        _httpClient = httpClient;
    }

    public async Task<SynologyMonitoringResponse> GetMonitoringAsync()
    {
        var host = _configuration["Synology:Host"]
            ?? throw new InvalidOperationException("Synology host is not configured.");

        var port = _configuration["Synology:Port"] ?? "5001";

        var username = _configuration["Synology:Username"]
            ?? throw new InvalidOperationException("Synology username is not configured.");

        var password = _configuration["Synology:Password"];

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("Synology API password is not configured.");
        }

        var baseUrl = $"https://{host}:{port}/webapi/entry.cgi";

        var loginParameters = new Dictionary<string, string>
        {
            ["api"] = "SYNO.API.Auth",
            ["version"] = "6",
            ["method"] = "login",
            ["account"] = username,
            ["passwd"] = password,
            ["session"] = "BDIP",
            ["format"] = "sid",
            ["enable_syno_token"] = "yes"
        };

        using var loginResponse =
            await _httpClient.GetAsync(BuildUrl(baseUrl, loginParameters));

        loginResponse.EnsureSuccessStatusCode();

        var loginJson =
            await loginResponse.Content.ReadAsStringAsync();

        using var loginDocument =
            JsonDocument.Parse(loginJson);

        var loginRoot = loginDocument.RootElement;

        if (!loginRoot.GetProperty("success").GetBoolean())
        {
            throw new InvalidOperationException(
                $"Synology authentication failed: {loginJson}");
        }

        var loginData = loginRoot.GetProperty("data");

        var sid = loginData.GetProperty("sid").GetString() ?? "";
        var synoToken =
            loginData.TryGetProperty("synotoken", out var tokenElement)
                ? tokenElement.GetString() ?? ""
                : "";

        try
        {
            var volumeParameters = new Dictionary<string, string>
            {
                ["api"] = "SYNO.Core.Storage.Volume",
                ["version"] = "1",
                ["method"] = "list",
                ["limit"] = "-1",
                ["offset"] = "0",
                ["location"] = "internal",
                ["_sid"] = sid,
                ["SynoToken"] = synoToken
            };

            using var volumeResponse =
                await _httpClient.GetAsync(
                    BuildUrl(baseUrl, volumeParameters));

            volumeResponse.EnsureSuccessStatusCode();

            var volumeJson =
                await volumeResponse.Content.ReadAsStringAsync();

            using var volumeDocument =
                JsonDocument.Parse(volumeJson);

            var volumeRoot = volumeDocument.RootElement;

            if (!volumeRoot.GetProperty("success").GetBoolean())
            {
                throw new InvalidOperationException(
                    $"Synology storage query failed: {volumeJson}");
            }

            var volumes =
                volumeRoot
                    .GetProperty("data")
                    .GetProperty("volumes");

            if (volumes.GetArrayLength() == 0)
            {
                throw new InvalidOperationException(
                    "Synology returned no internal volumes.");
            }

            var volume = volumes[0];

            var systemHealthParameters = new Dictionary<string, string>
            {
                ["api"] = "SYNO.Core.System.SystemHealth",
                ["version"] = "1",
                ["method"] = "get",
                ["_sid"] = sid,
                ["SynoToken"] = synoToken
            };

            using var systemHealthResponse =
                await _httpClient.GetAsync(
                    BuildUrl(baseUrl, systemHealthParameters));

            systemHealthResponse.EnsureSuccessStatusCode();

            var systemHealthJson =
                await systemHealthResponse.Content.ReadAsStringAsync();

            using var systemHealthDocument =
                JsonDocument.Parse(systemHealthJson);

            var systemHealthRoot =
                systemHealthDocument.RootElement;

            var systemHealth =
                new SynologySystemHealth();

            if (systemHealthRoot.GetProperty("success").GetBoolean() &&
                systemHealthRoot.TryGetProperty("data", out var systemHealthData))
            {
                systemHealth.Hostname =
                    systemHealthData.TryGetProperty("hostname", out var hostname)
                        ? hostname.GetString() ?? ""
                        : "";

                systemHealth.Uptime =
                    systemHealthData.TryGetProperty("uptime", out var uptime)
                        ? uptime.GetString() ?? ""
                        : "";

                systemHealth.Healthy = true;

                if (systemHealthData.TryGetProperty("interfaces", out var interfaces) &&
                    interfaces.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in interfaces.EnumerateArray())
                    {
                        systemHealth.Interfaces.Add(
                            new SynologyNetworkInterface
                            {
                                Id =
                                    item.TryGetProperty("id", out var id)
                                        ? id.GetString() ?? ""
                                        : "",

                                Ip =
                                    item.TryGetProperty("ip", out var ip)
                                        ? ip.GetString() ?? ""
                                        : "",

                                Type =
                                    item.TryGetProperty("type", out var type)
                                        ? type.GetString() ?? ""
                                        : ""
                            });
                    }
                }
            }

            var connectionParameters = new Dictionary<string, string>
            {
                ["api"] = "SYNO.Core.CurrentConnection",
                ["version"] = "1",
                ["method"] = "list",
                ["_sid"] = sid,
                ["SynoToken"] = synoToken
            };

            using var connectionResponse =
                await _httpClient.GetAsync(
                    BuildUrl(baseUrl, connectionParameters));

            connectionResponse.EnsureSuccessStatusCode();

            var connectionJson =
                await connectionResponse.Content.ReadAsStringAsync();

            using var connectionDocument =
                JsonDocument.Parse(connectionJson);

            var connectionRoot =
                connectionDocument.RootElement;

            var connections =
                new List<SynologyConnectionActivity>();

            if (connectionRoot.GetProperty("success").GetBoolean())
            {
                if (connectionRoot.TryGetProperty("data", out var connectionData))
                {
                    JsonElement connectionItems;

                    if (connectionData.TryGetProperty("items", out connectionItems) &&
                        connectionItems.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in connectionItems.EnumerateArray())
                        {
                            connections.Add(
                                new SynologyConnectionActivity
                                {
                                    User =
                                        item.TryGetProperty("who", out var who)
                                            ? who.GetString() ?? ""
                                            : "",

                                    SourceIp =
                                        item.TryGetProperty("from", out var from)
                                            ? from.GetString() ?? ""
                                            : "",

                                    Protocol =
                                        item.TryGetProperty("protocol", out var protocol)
                                            ? protocol.GetString() ?? ""
                                            : "",

                                    Type =
                                        item.TryGetProperty("type", out var type)
                                            ? type.GetString() ?? ""
                                            : "",

                                    Application =
                                        item.TryGetProperty("descr", out var descr)
                                            ? descr.GetString() ?? ""
                                            : "",

                                    Time =
                                        item.TryGetProperty("time", out var time)
                                            ? time.GetString() ?? ""
                                            : "",

                                    FirstLoginTime =
                                        item.TryGetProperty("first_login_time", out var firstLoginTime)
                                            ? firstLoginTime.GetString() ?? ""
                                            : "",

                                    CurrentConnected =
                                        item.TryGetProperty("is_current_connected", out var currentConnected) &&
                                        currentConnected.ValueKind == JsonValueKind.True,

                                    Location =
                                        item.TryGetProperty("location", out var location)
                                            ? location.GetString() ?? ""
                                            : "",

                                    UserAgent =
                                        item.TryGetProperty("user_agent", out var userAgent)
                                            ? userAgent.GetString() ?? ""
                                            : "",

                                    Pid =
                                        item.TryGetProperty("pid", out var pid) &&
                                        pid.TryGetInt32(out var pidValue)
                                            ? pidValue
                                            : 0,

                                    DeviceId =
                                        item.TryGetProperty("did", out var did)
                                            ? did.GetString() ?? ""
                                            : "",

                                    CanBeKicked =
                                        item.TryGetProperty("can_be_kicked", out var canBeKicked) &&
                                        canBeKicked.ValueKind == JsonValueKind.True,

                                    IsAmfa =
                                        item.TryGetProperty("is_amfa", out var isAmfa) &&
                                        isAmfa.ValueKind == JsonValueKind.True,

                                    IsOtpTrusted =
                                        item.TryGetProperty("is_otp_trusted", out var isOtpTrusted) &&
                                        isOtpTrusted.ValueKind == JsonValueKind.True
                                });
                        }
                    }
                }
            }

            // =================================================
            // SYNOLOGY STORAGE MANAGER HARDWARE
            // =================================================

            var hardware = new SynologyHardware();

            var storageParameters = new Dictionary<string, string>
            {
                ["api"] = "SYNO.Storage.CGI.Storage",
                ["version"] = "1",
                ["method"] = "load_info",
                ["_sid"] = sid,
                ["SynoToken"] = synoToken
            };

            using var storageResponse =
                await _httpClient.GetAsync(
                    BuildUrl(baseUrl, storageParameters));

            storageResponse.EnsureSuccessStatusCode();

            var storageJson =
                await storageResponse.Content.ReadAsStringAsync();

            using var storageDocument =
                JsonDocument.Parse(storageJson);

            var storageRoot = storageDocument.RootElement;

            if (storageRoot.TryGetProperty("success", out var storageSuccess) &&
                storageSuccess.GetBoolean() &&
                storageRoot.TryGetProperty("data", out var storageData))
            {
                if (storageData.TryGetProperty("env", out var env) &&
                    env.TryGetProperty("bay_number", out var bayNumber))
                {
                    if (bayNumber.ValueKind == JsonValueKind.Number &&
                        bayNumber.TryGetInt32(out var bay))
                    {
                        hardware.BayCount = bay;
                    }
                    else if (bayNumber.ValueKind == JsonValueKind.String &&
                             int.TryParse(bayNumber.GetString(), out var bayString))
                    {
                        hardware.BayCount = bayString;
                    }
                }

                if (storageData.TryGetProperty("disks", out var disks) &&
                    disks.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in disks.EnumerateArray())
                    {
                        var disk = new SynologyDisk
                        {
                            Id = item.TryGetProperty("id", out var id)
                                ? id.ToString()
                                : "",

                            Name = item.TryGetProperty("name", out var name)
                                ? name.ToString()
                                : "",

                            Model = item.TryGetProperty("model", out var model)
                                ? model.ToString()
                                : "",

                            Vendor = item.TryGetProperty("vendor", out var vendor)
                                ? vendor.ToString()
                                : "",

                            Serial = item.TryGetProperty("serial", out var serial)
                                ? serial.ToString()
                                : "",

                            Status = item.TryGetProperty("status", out var status)
                                ? status.ToString()
                                : "",

                            SmartStatus =
                                item.TryGetProperty("smart_status", out var smart)
                                    ? smart.ToString()
                                    : "",

                            IsSsd =
                                item.TryGetProperty("isSsd", out var isSsd) &&
                                isSsd.ValueKind == JsonValueKind.True
                        };

                        if (item.TryGetProperty("size_total", out var size))
                        {
                            if (size.ValueKind == JsonValueKind.Number)
                            {
                                size.TryGetInt64(out var value);
                                disk.CapacityBytes = value;
                            }
                            else if (
                                size.ValueKind == JsonValueKind.String &&
                                long.TryParse(size.GetString(), out var value))
                            {
                                disk.CapacityBytes = value;
                            }
                        }

                        if (item.TryGetProperty("temp", out var temp))
                        {
                            if (temp.ValueKind == JsonValueKind.Number)
                            {
                                temp.TryGetDouble(out var value);
                                disk.Temperature = value;
                            }
                            else if (
                                temp.ValueKind == JsonValueKind.String &&
                                double.TryParse(temp.GetString(), out var value))
                            {
                                disk.Temperature = value;
                            }
                        }

                        if (item.TryGetProperty("remain_life", out var life))
                        {
                            if (life.ValueKind == JsonValueKind.Object &&
                                life.TryGetProperty("value", out var value))
                            {
                                disk.RemainingLife = value.ToString();
                            }
                            else
                            {
                                disk.RemainingLife = life.ToString();
                            }
                        }

                        hardware.Disks.Add(disk);

                        var statusText =
                            $"{disk.Status} {disk.SmartStatus}"
                                .ToLowerInvariant();

                        if (
                            statusText.Contains("normal") ||
                            statusText.Contains("healthy") ||
                            statusText.Contains("good")
                        )
                        {
                            hardware.HealthyDisks++;
                        }
                        else if (
                            statusText.Contains("warning") ||
                            statusText.Contains("degrad")
                        )
                        {
                            hardware.WarningDisks++;
                        }
                        else if (
                            statusText.Contains("fail") ||
                            statusText.Contains("error") ||
                            statusText.Contains("critical")
                        )
                        {
                            hardware.FailedDisks++;
                        }
                    }

                    hardware.DiskCount = hardware.Disks.Count;
                }

                if (storageData.TryGetProperty("ssdCaches", out var caches) &&
                    caches.ValueKind == JsonValueKind.Array &&
                    caches.GetArrayLength() > 0)
                {
                    var cache = caches[0];

                    hardware.SsdCache.Enabled = true;

                    hardware.SsdCache.Status =
                        cache.TryGetProperty("status", out var status)
                            ? status.ToString()
                            : "";

                    hardware.SsdCache.RaidType =
                        cache.TryGetProperty("raidType", out var raid)
                            ? raid.ToString()
                            : "";

                    if (
                        cache.TryGetProperty("diskCount", out var count) &&
                        count.TryGetInt32(out var diskCount)
                    )
                    {
                        hardware.SsdCache.DiskCount = diskCount;
                    }

                    if (
                        cache.TryGetProperty("hitRate", out var hit) &&
                        hit.TryGetDouble(out var hitRate)
                    )
                    {
                        hardware.SsdCache.HitRate = hitRate;
                    }
                }
            }

            return new SynologyMonitoringResponse
            {
                Connections = connections,

                Hardware = hardware,

                Online = true,

                Model = "RS3617RPxs",

                DsmVersion = "DSM 7.3-86003",

                Volume = new SynologyVolume
                {
                    Name = volume.GetProperty("display_name").GetString() ?? "",
                    Path = volume.GetProperty("volume_path").GetString() ?? "",
                    FileSystem = volume.GetProperty("fs_type").GetString() ?? "",
                    RaidType = volume.GetProperty("raid_type").GetString() ?? "",
                    Status = volume.GetProperty("status").GetString() ?? "",
                    TotalBytes =
                        long.Parse(
                            volume.GetProperty("size_total_byte").GetString() ?? "0"),
                    FreeBytes =
                        long.Parse(
                            volume.GetProperty("size_free_byte").GetString() ?? "0")
                }
                ,

                SystemHealth = systemHealth
            };
        }
        finally
        {
            var logoutParameters = new Dictionary<string, string>
            {
                ["api"] = "SYNO.API.Auth",
                ["version"] = "6",
                ["method"] = "logout",
                ["session"] = "BDIP",
                ["_sid"] = sid
            };

            try
            {
                await _httpClient.GetAsync(
                    BuildUrl(baseUrl, logoutParameters));
            }
            catch
            {
                // Logout failure must not hide successful monitoring data.
            }
        }
    }

    private static string BuildUrl(
        string baseUrl,
        Dictionary<string, string> parameters)
    {
        var query = string.Join(
            "&",
            parameters.Select(
                x =>
                    $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));

        return $"{baseUrl}?{query}";
    }
}
