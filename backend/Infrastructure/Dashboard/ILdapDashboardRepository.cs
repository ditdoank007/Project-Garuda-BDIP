namespace BDIP.Infrastructure.Dashboard;

public interface ILdapDashboardRepository
{
    Task<int> CountUsersAsync();

    Task<int> CountGroupsAsync();

    Task<int> CountUnitsAsync();

    Task<int> CountApplicationsAsync();

    Task<bool> IsHealthyAsync();
}
