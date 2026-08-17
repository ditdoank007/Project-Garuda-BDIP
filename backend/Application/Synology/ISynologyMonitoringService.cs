using BDIP.Contracts.Synology;

namespace BDIP.Application.Synology;

public interface ISynologyMonitoringService
{
    Task<SynologyMonitoringResponse> GetMonitoringAsync();
}
