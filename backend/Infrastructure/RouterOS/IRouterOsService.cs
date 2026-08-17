using System.Collections.Generic;

namespace BDIP.Infrastructure.RouterOS;

public interface IRouterOsService
{
    Task<bool> TestConnectionAsync();

    Task<List<RouterOsActiveSession>> GetHotspotActiveAsync();

    Task<List<RouterOsActiveSession>> GetPppActiveAsync();

    Task<List<RouterOsOvpnInterface>> GetOvpnInterfacesAsync();

    Task<List<RouterOsOvpnTraffic>> GetOvpnTrafficAsync();

    Task<object> GetPppRawAsync();

    Task<object> GetOvpnInterfacesRawAsync();

    Task DisconnectHotspotSessionAsync(string sessionId);
}