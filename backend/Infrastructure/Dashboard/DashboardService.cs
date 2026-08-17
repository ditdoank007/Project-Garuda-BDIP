using BDIP.Infrastructure.Synology;
using Microsoft.Extensions.Configuration;
using BDIP.Application.Dashboard;
using BDIP.Contracts.Dashboard;
using BDIP.Application.Users;
using BDIP.Application.NAP;
using BDIP.Application.Sessions;
using BDIP.Application.Synology;
using BDIP.Infrastructure.RouterOS;
using System.Linq;

namespace BDIP.Infrastructure.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly ILdapDashboardRepository _ldapDashboardRepository;

    private readonly IConfiguration _configuration;

    private readonly SynologySnmpService _synologySnmpService;

    private readonly IUserService _userService;
    private readonly IPolicyService _policyService;
    private readonly ISessionService _sessionService;
    private readonly IRouterOsService _routerOsService;
    private readonly ISynologyMonitoringService _synologyMonitoringService;

    public DashboardService(
        ILdapDashboardRepository ldapDashboardRepository,
        IConfiguration configuration,
        SynologySnmpService synologySnmpService,
        IUserService userService,
        IPolicyService policyService,
        ISessionService sessionService,
        IRouterOsService routerOsService,
        ISynologyMonitoringService synologyMonitoringService)
    {
        _ldapDashboardRepository = ldapDashboardRepository;
        _configuration = configuration;
        _synologySnmpService = synologySnmpService;
        _userService = userService;
        _policyService = policyService;
        _sessionService = sessionService;
        _routerOsService = routerOsService;
        _synologyMonitoringService = synologyMonitoringService;
    }

    public async Task<DashboardResponse> GetDashboardAsync()
    {
        var totalUsers = await _userService.CountUsersAsync();

        var policies = await _policyService.GetAllAsync();

        var sessions = await _sessionService.GetSessionsAsync();

        var hotspot = await _routerOsService.GetHotspotActiveAsync();

        var ppp = await _routerOsService.GetPppActiveAsync();

        var routerLookup =
            hotspot
                .GroupBy(
                    x => x.User,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.First(),
                    StringComparer.OrdinalIgnoreCase);

        foreach (var session in sessions.Sessions)
        {
            session.IsRouterActive =
                routerLookup.ContainsKey(session.Username);
        }

        var groups = await _ldapDashboardRepository.CountGroupsAsync();

        var units = await _ldapDashboardRepository.CountUnitsAsync();

        var applications = await _ldapDashboardRepository.CountApplicationsAsync();

        var ldapHealthy = await _ldapDashboardRepository.IsHealthyAsync();

        var synology = await _synologyMonitoringService.GetMonitoringAsync();

        var synologyHost =
            _configuration["Synology:Host"]
            ?? throw new InvalidOperationException(
                "Synology host is not configured.");

        var snmp =
            await _synologySnmpService.GetSnapshotAsync(
                synologyHost);

        return new DashboardResponse
        {
            Stats =
            {
                TotalUsers = totalUsers,
                ActiveSessions =
                    hotspot.Count + ppp.Count,
                HotspotSessions = hotspot.Count,
                VpnSessions = ppp.Count,
                TotalPolicies = policies.Count(),
                NasOnline = synology.Online ? 1 : 0,
                Applications = applications,
                Groups = groups,
                Units = units,
                Ldap = ldapHealthy ? "Healthy" : "Unavailable"
            },

            Synology = new SynologyMonitoring
            {
                Online = synology.Online,
                Model = synology.Model,
                DsmVersion = synology.DsmVersion,
                VolumeName = synology.Volume.Name,
                VolumePath = synology.Volume.Path,
                FileSystem = synology.Volume.FileSystem,
                RaidType = synology.Volume.RaidType,
                Status = synology.Volume.Status,
                TotalBytes = synology.Volume.TotalBytes,
                UsedBytes = synology.Volume.UsedBytes,
                FreeBytes = synology.Volume.FreeBytes,
                UsedPercent = synology.Volume.UsedPercent,

                Performance = new SynologyPerformance
                {
                    ReadBytesPerSecond = snmp.ReadBytesPerSecond,
                    WriteBytesPerSecond = snmp.WriteBytesPerSecond,
                    ReadIops = snmp.ReadIops,
                    WriteIops = snmp.WriteIops
                },

                SystemResources = new SynologySystemResources
                {
                    CpuPercent = snmp.CpuPercent,
                    MemoryPercent = snmp.MemoryPercent,
                    TemperatureC = snmp.TemperatureC,
                    FanStatus =
                        snmp.SystemFanStatus == 1 &&
                        snmp.CpuFanStatus == 1
                            ? "OK"
                            : "WARNING"
                },

                StorageHealth = new SynologyStorageHealth
                {
                    RaidStatus = synology.Hardware.PoolStatus,
                    FilesystemStatus = synology.Volume.FileSystem,
                    DiskHealth =
                        $"{synology.Hardware.HealthyDisks}/" +
                        $"{synology.Hardware.DiskCount} OK",
                    BadSectors = null
                },

                Hardware = new SynologyHardware
                {
                    BayCount = synology.Hardware.BayCount,
                    DiskCount = synology.Hardware.DiskCount,
                    HealthyDisks = synology.Hardware.HealthyDisks,
                    WarningDisks = synology.Hardware.WarningDisks,
                    FailedDisks = synology.Hardware.FailedDisks,
                    PoolStatus = synology.Hardware.PoolStatus,
                    PoolRaidType = synology.Hardware.PoolRaidType,
                    PoolDiskCount = synology.Hardware.PoolDiskCount,
                    PoolTotalBytes = synology.Hardware.PoolTotalBytes,

                    SsdCache = new SynologySsdCache
                    {
                        Enabled = synology.Hardware.SsdCache.Enabled,
                        Status = synology.Hardware.SsdCache.Status,
                        RaidType = synology.Hardware.SsdCache.RaidType,
                        DiskCount = synology.Hardware.SsdCache.DiskCount,
                        HitRate = synology.Hardware.SsdCache.HitRate
                    },

                    Disks = synology.Hardware.Disks
                        .Select(d => new SynologyDisk
                        {
                            Id = d.Id,
                            Name = d.Name,
                            Model = d.Model,
                            Vendor = d.Vendor,
                            Serial = d.Serial,
                            CapacityBytes = d.CapacityBytes,
                            Status = d.Status,
                            SmartStatus = d.SmartStatus,
                            Temperature = d.Temperature,
                            IsSsd = d.IsSsd,
                            RemainingLife = d.RemainingLife
                        })
                        .ToList()
                },

                SystemHealth = synology.SystemHealth
            },

            Activities = new List<DashboardActivity>()
        };
    }
}
