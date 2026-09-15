using BDIP.Contracts.ExternalLogServer;

namespace BDIP.Application.ExternalLogServer;

public interface IExternalLogServerConfigService
{
    Task<ExternalLogServerConfigResponse> GetAsync(
        CancellationToken cancellationToken = default);

    Task<ExternalLogServerConfigResponse> UpdateAsync(
        ExternalLogServerConfigRequest request,
        CancellationToken cancellationToken = default);
}
