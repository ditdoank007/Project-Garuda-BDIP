using Microsoft.Extensions.Options;
using tik4net;

namespace BDIP.Infrastructure.RouterOS;

public sealed class RouterOsService
    : IRouterOsService
{
    private readonly RouterOsOptions _options;

    public RouterOsService(
        IOptions<RouterOsOptions> options)
    {
        _options = options.Value;
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

                    Server =
                        x.GetResponseFieldOrDefault(
                            "server",
                            string.Empty)
                })
                .ToList();

            return Task.FromResult(sessions);
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