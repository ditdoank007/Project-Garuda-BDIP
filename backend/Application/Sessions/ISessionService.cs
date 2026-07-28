using BDIP.Contracts.Sessions;

namespace BDIP.Application.Sessions;

public interface ISessionService
{
    Task<SessionListResponse> GetSessionsAsync(
        CancellationToken cancellationToken = default);
}
