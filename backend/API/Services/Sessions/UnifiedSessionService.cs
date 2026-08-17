using BDIP.Application.Sessions;
using BDIP.Contracts.Sessions;
using BDIP.Infrastructure.RouterOS;

namespace BDIP.API.Services.Sessions;

public sealed class UnifiedSessionService
{
    private readonly ISessionService _sessionService;
    private readonly IRouterOsService _routerOsService;

    public UnifiedSessionService(
        ISessionService sessionService,
        IRouterOsService routerOsService)
    {
        _sessionService = sessionService;
        _routerOsService = routerOsService;
    }

    public async Task<SessionListResponse> GetSessionsAsync(
        CancellationToken cancellationToken = default)
    {
        var result =
            await _sessionService.GetSessionsAsync(
                cancellationToken);

        var hotspotSessions =
            await _routerOsService.GetHotspotActiveAsync();

        var vpnSessions =
            await _routerOsService.GetPppActiveAsync();

        var ovpnTraffic =
            await _routerOsService.GetOvpnTrafficAsync();

        var liveSessions =
            hotspotSessions
                .Concat(vpnSessions)
                .ToList();

        /*
         * RouterOS is the source of truth for ACTIVE sessions.
         *
         * RADIUS remains the source of historical accounting.
         *
         * Only an actually open RADIUS record may be enriched
         * from RouterOS. Historical records for the same username
         * must never become ACTIVE merely because the username
         * currently exists on RouterOS.
         */

        foreach (var live in liveSessions)
        {
            var radiusActive =
                result.Sessions.FirstOrDefault(
                    x =>
                        x.AcctStopTime == null &&
                        string.Equals(
                            x.Username,
                            live.User,
                            StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(
                            x.FramedIpAddress,
                            live.Address,
                            StringComparison.OrdinalIgnoreCase));

            if (radiusActive != null)
            {
                EnrichFromRouterOs(radiusActive, live);

                if (string.Equals(
                        live.Server,
                        "ovpn",
                        StringComparison.OrdinalIgnoreCase))
                {
                    EnrichOvpnTraffic(
                        radiusActive,
                        ovpnTraffic);
                }

                continue;
            }

            /*
             * RouterOS live session has no matching open RADIUS
             * accounting record. Add it as a synthetic live session
             * so the Session page always reflects actual connectivity.
             */
            var syntheticSession =
                CreateSyntheticLiveSession(live);

            if (string.Equals(
                    live.Server,
                    "ovpn",
                    StringComparison.OrdinalIgnoreCase))
            {
                EnrichOvpnTraffic(
                    syntheticSession,
                    ovpnTraffic);
            }

            result.Sessions.Add(
                syntheticSession);
        }

        result.Total = result.Sessions.Count;

        return result;
    }

    public async Task<(bool Found, long RxBytes, long TxBytes)>
        GetLiveTrafficAsync(
            string username,
            string address,
            string server)
    {
        if (string.Equals(
                server,
                "ovpn",
                StringComparison.OrdinalIgnoreCase))
        {
            var traffic =
                await _routerOsService.GetOvpnTrafficAsync();

            var match =
                traffic.FirstOrDefault(
                    x =>
                        string.Equals(
                            x.User,
                            username,
                            StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(
                            x.Address,
                            address,
                            StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                return (false, 0, 0);
            }

            return (
                true,
                match.RxBytes,
                match.TxBytes);
        }

        if (string.Equals(
                server,
                "ppp",
                StringComparison.OrdinalIgnoreCase))
        {
            var sessions =
                await _routerOsService.GetPppActiveAsync();

            var match =
                sessions.FirstOrDefault(
                    x =>
                        string.Equals(
                            x.User,
                            username,
                            StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(
                            x.Address,
                            address,
                            StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                return (false, 0, 0);
            }

            return (
                true,
                match.BytesIn,
                match.BytesOut);
        }

        var hotspotSessions =
            await _routerOsService.GetHotspotActiveAsync();

        var hotspotMatch =
            hotspotSessions.FirstOrDefault(
                x =>
                    string.Equals(
                        x.User,
                        username,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        x.Address,
                        address,
                        StringComparison.OrdinalIgnoreCase));

        if (hotspotMatch == null)
        {
            return (false, 0, 0);
        }

        return (
            true,
            hotspotMatch.BytesIn,
            hotspotMatch.BytesOut);
    }

    private static void EnrichOvpnTraffic(
        SessionResponse session,
        List<RouterOsOvpnTraffic> traffic)
    {
        var match =
            traffic.FirstOrDefault(
                x =>
                    string.Equals(
                        x.User,
                        session.Username,
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        x.Address,
                        session.FramedIpAddress,
                        StringComparison.OrdinalIgnoreCase));

        if (match == null)
        {
            return;
        }

        session.RouterOsInterface =
            match.InterfaceName;

        session.RouterOsRxBytes =
            match.RxBytes;

        session.RouterOsTxBytes =
            match.TxBytes;
    }

    private static void EnrichFromRouterOs(
        SessionResponse session,
        RouterOsActiveSession live)
    {
        session.RouterOsId = live.Id;
        session.RouterAddress = live.Address;
        session.MacAddress = live.MacAddress;
        session.RouterServer = live.Server;
        session.IsRouterActive = true;

        session.RouterOsRxBytes = live.BytesIn;
        session.RouterOsTxBytes = live.BytesOut;

        if (string.Equals(
                live.Server,
                "ovpn",
                StringComparison.OrdinalIgnoreCase))
        {
            session.FramedProtocol = "PPP";
            session.ServiceType = "Framed-User";
        }
        else
        {
            session.ServiceType = "Hotspot";
        }
    }

    private static SessionResponse CreateSyntheticLiveSession(
        RouterOsActiveSession live)
    {
        var isVpn =
            string.Equals(
                live.Server,
                "ovpn",
                StringComparison.OrdinalIgnoreCase);

        return new SessionResponse
        {
            RadAcctId = 0,

            AcctSessionId = "",

            Username = live.User,

            NasIpAddress = isVpn
                ? "10.24.24.1"
                : "",

            NasPortId = "",
            NasPortType = "",

            AcctStartTime = null,
            AcctUpdateTime = null,
            AcctStopTime = null,

            AcctSessionTime = null,
            AcctInputOctets = null,
            AcctOutputOctets = null,

            AcctTerminateCause = "",

            CalledStationId = "",
            CallingStationId = live.MacAddress,

            ServiceType = isVpn
                ? "Framed-User"
                : "Hotspot",

            FramedProtocol = isVpn
                ? "PPP"
                : "",

            FramedIpAddress = live.Address,

            RouterOsId = live.Id,
            RouterAddress = live.Address,
            MacAddress = live.MacAddress,
            RouterServer = live.Server,
            IsRouterActive = true
        };
    }
}
