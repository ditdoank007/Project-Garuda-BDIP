using System.Collections.Generic;

namespace BDIP.Infrastructure.RouterOS;

public interface IRouterOsService
{
    Task<bool> TestConnectionAsync();

    Task<List<RouterOsActiveSession>> GetHotspotActiveAsync();

    Task DisconnectHotspotSessionAsync(string sessionId);
}