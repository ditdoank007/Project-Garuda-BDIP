using BDIP.Application.Dashboard;
using BDIP.Contracts.Dashboard;
using BDIP.Application.Users;
using BDIP.Application.NAP;
using BDIP.Application.Sessions;
using BDIP.Infrastructure.RouterOS;
using System.Linq;

namespace BDIP.Infrastructure.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly ILdapDashboardRepository _ldapDashboardRepository;

    private readonly IUserService _userService;
    private readonly IPolicyService _policyService;
    private readonly ISessionService _sessionService;
    private readonly IRouterOsService _routerOsService;

    public DashboardService(
        ILdapDashboardRepository ldapDashboardRepository,
        IUserService userService,
        IPolicyService policyService,
        ISessionService sessionService,
        IRouterOsService routerOsService)
    {
        _ldapDashboardRepository = ldapDashboardRepository;
        _userService = userService;
        _policyService = policyService;
        _sessionService = sessionService;
        _routerOsService = routerOsService;
    }

    public async Task<DashboardResponse> GetDashboardAsync()
    {
        var totalUsers = await _userService.CountUsersAsync();

        var policies = await _policyService.GetAllAsync();

        var sessions = await _sessionService.GetSessionsAsync();

        var hotspot = await _routerOsService.GetHotspotActiveAsync();

        var groups = await _ldapDashboardRepository.CountGroupsAsync();

        var units = await _ldapDashboardRepository.CountUnitsAsync();

        var applications = await _ldapDashboardRepository.CountApplicationsAsync();

        var ldapHealthy = await _ldapDashboardRepository.IsHealthyAsync();

        return new DashboardResponse
        {
            Stats =
            {
                TotalUsers = totalUsers,
                ActiveSessions = sessions.Total,
                HotspotSessions = hotspot.Count,
                VpnSessions = 0,
                TotalPolicies = policies.Count(),
                NasOnline = ldapHealthy ? 1 : 0,
                Applications = applications,
                Groups = groups,
                Units = units,
                Ldap = ldapHealthy ? "Healthy" : "Unavailable"
            },

            Activities = new List<DashboardActivity>()
        };
    }
}
