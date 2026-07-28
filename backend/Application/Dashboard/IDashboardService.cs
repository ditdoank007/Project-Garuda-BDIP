using BDIP.Contracts.Dashboard;

namespace BDIP.Application.Dashboard;

public interface IDashboardService
{
    Task<DashboardResponse> GetDashboardAsync();
}