using Microsoft.Extensions.Options;
using tik4net;

namespace BDIP.Infrastructure.RouterOS;

public sealed class RouterOsService
    : IRouterOsService
{
    private readonly RouterOsOptions _options;
    private readonly RouterOsOvpnOptions _ovpnOptions;

    public RouterOsService(
        IOptions<RouterOsOptions> options,
        IOptions<RouterOsOvpnOptions> ovpnOptions)
    {
        _options = options.Value;
        _ovpnOptions = ovpnOptions.Value;
    }

    public Task<bool> TestConnectionAsync()
    {
        try
        {
            using var connection =
                RouterOsConnection.Create(_options);

            return Task.FromResult(
                connection.IsOpened);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }
    public Task<List<RouterOsActiveSession>> GetHotspotActiveAsync()
    {
        using var connection =
            ConnectionFactory.OpenConnection(
                TikConnectionType.Api,
                _options.Host,
                _options.Port,
                _options.Username,
                _options.Password);

            var command =
                connection.CreateCommand(
                    "/ip/hotspot/active/print");

            var result =
                command.ExecuteList();

            var sessions =
                result.Select(x => new RouterOsActiveSession
                {
                    Id = x.GetId(),

                    User =
                        x.GetResponseFieldOrDefault(
                            "user",
                            string.Empty),

                    Address =
                        x.GetResponseFieldOrDefault(
                            "address",
                            string.Empty),

                    MacAddress =
                        x.GetResponseFieldOrDefault(
                            "mac-address",
                            string.Empty),

                    Uptime =
                        x.GetResponseFieldOrDefault(
                            "uptime",
                            string.Empty),

                    BytesIn =
                        long.TryParse(
                            x.GetResponseFieldOrDefault(
                                "bytes-in",
                                "0"),
                            out var bytesIn)
                                ? bytesIn
                                : 0,

                    BytesOut =
                        long.TryParse(
                            x.GetResponseFieldOrDefault(
                                "bytes-out",
                                "0"),
                            out var bytesOut)
                                ? bytesOut
                                : 0,

                    Server =
                        x.GetResponseFieldOrDefault(
                            "server",
                            string.Empty)
                })
                .ToList();

            return Task.FromResult(sessions);
    }
    public Task<List<RouterOsActiveSession>> GetPppActiveAsync()
    {
        using var connection =
            ConnectionFactory.OpenConnection(
                TikConnectionType.Api,
                _ovpnOptions.Host,
                _ovpnOptions.Port,
                _ovpnOptions.Username,
                _ovpnOptions.Password);

        var command =
            connection.CreateCommand(
                "/ppp/active/print");

        var result =
            command.ExecuteList();

        var sessions =
            result.Select(x => new RouterOsActiveSession
            {
                Id = x.GetId(),

                User =
                    x.GetResponseFieldOrDefault(
                        "name",
                        string.Empty),

                Address =
                    x.GetResponseFieldOrDefault(
                        "address",
                        string.Empty),

                MacAddress =
                    x.GetResponseFieldOrDefault(
                        "caller-id",
                        string.Empty),

                Uptime =
                    x.GetResponseFieldOrDefault(
                        "uptime",
                        string.Empty),

                BytesIn =
                    long.TryParse(
                        x.GetResponseFieldOrDefault(
                            "bytes-in",
                            "0"),
                        out var bytesIn)
                            ? bytesIn
                            : 0,

                BytesOut =
                    long.TryParse(
                        x.GetResponseFieldOrDefault(
                            "bytes-out",
                            "0"),
                        out var bytesOut)
                            ? bytesOut
                            : 0,

                Server =
                    x.GetResponseFieldOrDefault(
                        "service",
                        string.Empty)
            })
            .ToList();

        return Task.FromResult(sessions);
    }

    public Task<List<RouterOsOvpnInterface>> GetOvpnInterfacesAsync()
    {
        using var connection =
            ConnectionFactory.OpenConnection(
                TikConnectionType.Api,
                _ovpnOptions.Host,
                _ovpnOptions.Port,
                _ovpnOptions.Username,
                _ovpnOptions.Password);

        var command =
            connection.CreateCommand(
                "/interface/print");

        var result =
            command.ExecuteList();

        var interfaces =
            result
                .Select(x => new
                {
                    Id = x.GetId(),

                    Name =
                        x.GetResponseFieldOrDefault(
                            "name",
                            string.Empty),

                    Type =
                        x.GetResponseFieldOrDefault(
                            "type",
                            string.Empty),

                    Running =
                        string.Equals(
                            x.GetResponseFieldOrDefault(
                                "running",
                                "false"),
                            "true",
                            StringComparison.OrdinalIgnoreCase),

                    RxBytes =
                        long.TryParse(
                            x.GetResponseFieldOrDefault(
                                "rx-byte",
                                "0"),
                            out var rxBytes)
                                ? rxBytes
                                : 0,

                    TxBytes =
                        long.TryParse(
                            x.GetResponseFieldOrDefault(
                                "tx-byte",
                                "0"),
                            out var txBytes)
                                ? txBytes
                                : 0
                })
                .Where(x =>
                    string.Equals(
                        x.Type,
                        "ovpn-in",
                        StringComparison.OrdinalIgnoreCase))
                .Select(x => new RouterOsOvpnInterface
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = x.Type,
                    Running = x.Running,
                    RxBytes = x.RxBytes,
                    TxBytes = x.TxBytes
                })
                .ToList();

        return Task.FromResult(interfaces);
    }

    public Task<List<RouterOsOvpnTraffic>> GetOvpnTrafficAsync()
    {
        using var connection =
            ConnectionFactory.OpenConnection(
                TikConnectionType.Api,
                _ovpnOptions.Host,
                _ovpnOptions.Port,
                _ovpnOptions.Username,
                _ovpnOptions.Password);

        var pppCommand =
            connection.CreateCommand(
                "/ppp/active/print");

        var pppResult =
            pppCommand.ExecuteList();

        var interfaceCommand =
            connection.CreateCommand(
                "/interface/print");

        var interfaceResult =
            interfaceCommand.ExecuteList();

        var interfaces =
            interfaceResult
                .Select(x => new
                {
                    Name =
                        x.GetResponseFieldOrDefault(
                            "name",
                            string.Empty),

                    Type =
                        x.GetResponseFieldOrDefault(
                            "type",
                            string.Empty),

                    Running =
                        string.Equals(
                            x.GetResponseFieldOrDefault(
                                "running",
                                "false"),
                            "true",
                            StringComparison.OrdinalIgnoreCase),

                    RxBytes =
                        long.TryParse(
                            x.GetResponseFieldOrDefault(
                                "rx-byte",
                                "0"),
                            out var rxBytes)
                                ? rxBytes
                                : 0,

                    TxBytes =
                        long.TryParse(
                            x.GetResponseFieldOrDefault(
                                "tx-byte",
                                "0"),
                            out var txBytes)
                                ? txBytes
                                : 0
                })
                .Where(x =>
                    string.Equals(
                        x.Type,
                        "ovpn-in",
                        StringComparison.OrdinalIgnoreCase))
                .ToList();

        static string NormalizeInterfaceName(string name)
        {
            var normalized =
                name.Trim()
                    .Trim('<', '>');

            const string prefix = "ovpn-";

            if (normalized.StartsWith(
                    prefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                normalized =
                    normalized[prefix.Length..];
            }

            if (normalized.EndsWith(
                    "-1",
                    StringComparison.OrdinalIgnoreCase))
            {
                normalized =
                    normalized[..^2];
            }

            return normalized;
        }

        var traffic =
            pppResult
                .Select(ppp =>
                {
                    var user =
                        ppp.GetResponseFieldOrDefault(
                            "name",
                            string.Empty);

                    var address =
                        ppp.GetResponseFieldOrDefault(
                            "address",
                            string.Empty);

                    var match =
                        interfaces.FirstOrDefault(
                            x =>
                                string.Equals(
                                    NormalizeInterfaceName(x.Name),
                                    user,
                                    StringComparison.OrdinalIgnoreCase));

                    return new RouterOsOvpnTraffic
                    {
                        User = user,
                        Address = address,

                        InterfaceName =
                            match?.Name ?? string.Empty,

                        Running =
                            match?.Running ?? false,

                        RxBytes =
                            match?.RxBytes ?? 0,

                        TxBytes =
                            match?.TxBytes ?? 0
                    };
                })
                .ToList();

        return Task.FromResult(traffic);
    }

    public Task<object> GetPppRawAsync()
    {
        using var connection =
            ConnectionFactory.OpenConnection(
                TikConnectionType.Api,
                _ovpnOptions.Host,
                _ovpnOptions.Port,
                _ovpnOptions.Username,
                _ovpnOptions.Password);

        var command =
            connection.CreateCommand(
                "/ppp/active/print");

        var result =
            command.ExecuteList();

        var data =
            result.Select(x => new
            {
                id = x.GetId(),
                words = x.Words
            })
            .ToList();

        return Task.FromResult<object>(data);
    }

    public Task<object> GetOvpnInterfacesRawAsync()
    {
        using var connection =
            ConnectionFactory.OpenConnection(
                TikConnectionType.Api,
                _ovpnOptions.Host,
                _ovpnOptions.Port,
                _ovpnOptions.Username,
                _ovpnOptions.Password);

        var command =
            connection.CreateCommand(
                "/interface/print");

        var result =
            command.ExecuteList();

        var data =
            result.Select(x => new
            {
                id = x.GetId(),
                words = x.Words
            })
            .ToList();

        return Task.FromResult<object>(data);
    }

    public Task DisconnectHotspotSessionAsync(
    string sessionId)
    {
        using var connection =
            RouterOsConnection.Create(_options);

        var command =
            connection.CreateCommand(
                "/ip/hotspot/active/remove");

        command.AddParameter(
            ".id",
            sessionId);

        command.ExecuteNonQuery();

        return Task.CompletedTask;
    }
}