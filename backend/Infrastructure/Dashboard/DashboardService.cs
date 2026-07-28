using BDIP.Application.Dashboard;
using BDIP.Contracts.Dashboard;

namespace BDIP.Infrastructure.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly ILdapDashboardRepository _ldapDashboardRepository;

    public DashboardService(ILdapDashboardRepository ldapDashboardRepository)
    {
        _ldapDashboardRepository = ldapDashboardRepository;
    }

    public async Task<DashboardResponse> GetDashboardAsync()
    {
        var users = await _ldapDashboardRepository.CountUsersAsync();
        var groups = await _ldapDashboardRepository.CountGroupsAsync();
        var units = await _ldapDashboardRepository.CountUnitsAsync();
        var applications = await _ldapDashboardRepository.CountApplicationsAsync();
        var ldapHealthy = await _ldapDashboardRepository.IsHealthyAsync();

        return new DashboardResponse
        {
            Stats =
            {
                Users = users,
                Groups = groups,
                Units = units,
                Applications = applications,
                Ldap = ldapHealthy ? "Healthy" : "Unavailable"
            },

            Activities = new List<DashboardActivity>()
        };
    }
}
