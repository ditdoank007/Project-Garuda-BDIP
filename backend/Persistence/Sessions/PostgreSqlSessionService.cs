using BDIP.Application.Sessions;
using BDIP.Contracts.Sessions;
using BDIP.Application.NAP;
using BDIP.Domain.NAP;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Persistence.Sessions;

public sealed class PostgreSqlSessionService : ISessionService
{
    private readonly PostgreSqlOptions _options;
    private readonly IUserNapService _userNapService;
    private readonly IPolicyService _policyService;

    public PostgreSqlSessionService(
        IOptions<PostgreSqlOptions> options,
        IUserNapService userNapService,
        IPolicyService policyService)
    {
        _options = options.Value;
        _userNapService = userNapService;
        _policyService = policyService;
    }

    public async Task<SessionListResponse> GetSessionsAsync(
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("=== PostgreSqlSessionService CALLED ===");
        Console.WriteLine("Opening PostgreSQL connection...");
        var connectionStringBuilder =
            new NpgsqlConnectionStringBuilder
            {
                Host = _options.Host,
                Port = _options.Port,
                Database = _options.Database,
                Username = _options.Username,
                Password = _options.Password,
                SslMode = SslMode.Disable,
                Timeout = 10,
                CommandTimeout = 15,
                ApplicationName = "BDIP Backend"
            };

        await using var dataSource =
            NpgsqlDataSource.Create(
                connectionStringBuilder.ConnectionString);

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    radacctid,
                    acctsessionid,
                    username,
                    nasipaddress,
                    nasportid,
                    nasporttype,
                    acctstarttime,
                    acctupdatetime,
                    acctstoptime,
                    acctsessiontime,
                    acctinputoctets,
                    acctoutputoctets,
                    acctterminatecause,
                    calledstationid,
                    callingstationid,
                    servicetype,
                    framedprotocol,
                    framedipaddress
                FROM public.radacct
                ORDER BY acctstarttime DESC NULLS LAST,
                         radacctid DESC;
                """);

    var result = new SessionListResponse();

    var userNapLookup =
        (await _userNapService.GetAllAsync())
            .ToDictionary(
                x => x.Uid,
                StringComparer.OrdinalIgnoreCase);

    var policyLookup =
        (await _policyService.GetAllAsync())
            .ToDictionary(
                x => x.Code,
                StringComparer.OrdinalIgnoreCase);
    await using var reader =
        await command.ExecuteReaderAsync(cancellationToken);

        Console.WriteLine("Query executed.");

        while (await reader.ReadAsync(cancellationToken))
        {
            var session = new SessionResponse
                {
                    RadAcctId =
                        reader.GetInt64(0),

                    AcctSessionId =
                        GetString(reader, 1),

                    Username =
                        GetString(reader, 2),

                    NasIpAddress =
                        GetString(reader, 3),

                    NasPortId =
                        GetString(reader, 4),

                    NasPortType =
                        GetString(reader, 5),

                    AcctStartTime =
                        GetDateTimeOffset(reader, 6),

                    AcctUpdateTime =
                        GetDateTimeOffset(reader, 7),

                    AcctStopTime =
                        GetDateTimeOffset(reader, 8),

                    AcctSessionTime =
                        GetInt64(reader, 9),

                    AcctInputOctets =
                        GetInt64(reader, 10),

                    AcctOutputOctets =
                        GetInt64(reader, 11),

                    AcctTerminateCause =
                        GetString(reader, 12),

                    CalledStationId =
                        GetString(reader, 13),

                    CallingStationId =
                        GetString(reader, 14),

                    ServiceType =
                        GetString(reader, 15),

                    FramedProtocol =
                        GetString(reader, 16),

                    FramedIpAddress =
                        GetString(reader, 17)
                        
                };
                if (userNapLookup.TryGetValue(session.Username, out var userNap))
                {
                    session.PolicyCode = userNap.PolicyCode ?? "";
                    session.DownloadRate = userNap.DownloadKbps;
                    session.UploadRate = userNap.UploadKbps;
                    session.SessionTimeout = userNap.SessionTimeout;
                    session.IdleTimeout = userNap.IdleTimeout;

                    if (!string.IsNullOrWhiteSpace(userNap.PolicyCode) &&
                        policyLookup.TryGetValue(userNap.PolicyCode, out var policy))
                    {
                        session.PolicyName = policy.Name;
                        session.SimultaneousUse = policy.SimultaneousUse;
                    }
                }                result.Sessions.Add(session);
        }

        result.Total = result.Sessions.Count;

        Console.WriteLine($"Returning {result.Total} sessions.");

        return result;
    }

    private static string GetString(
        NpgsqlDataReader reader,
        int ordinal)
    {
        return reader.IsDBNull(ordinal)
            ? ""
            : reader.GetValue(ordinal).ToString() ?? "";
    }

    private static long? GetInt64(
        NpgsqlDataReader reader,
        int ordinal)
    {
        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetInt64(ordinal);
    }

    private static DateTimeOffset? GetDateTimeOffset(
        NpgsqlDataReader reader,
        int ordinal)
    {
        return reader.IsDBNull(ordinal)
            ? null
            : reader.GetFieldValue<DateTimeOffset>(ordinal);
    }
}
