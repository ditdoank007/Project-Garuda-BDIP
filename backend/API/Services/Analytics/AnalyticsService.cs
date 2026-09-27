using BDIP.Contracts.Analytics;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;
using NpgsqlTypes;

namespace BDIP.API.Services.Analytics;

public sealed class AnalyticsService
{
    private readonly PostgreSqlOptions _options;

    public AnalyticsService(IOptions<PostgreSqlOptions> options)
    {
        _options = options.Value;
    }

    public async Task<AnalyticsResponse> GetAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        string? username,
        string? access,
        CancellationToken cancellationToken = default)
    {
        if (to <= from)
        {
            throw new ArgumentException(
                "Analytics period must have an end after the start.");
        }

        // Never project accounting data into the future.
        // This is especially important for open sessions (acctstoptime IS NULL),
        // because they otherwise appear to continue until the selected period end.
        var effectiveTo =
            to > DateTimeOffset.UtcNow
                ? DateTimeOffset.UtcNow
                : to;

        if (effectiveTo <= from)
        {
            return new AnalyticsResponse();
        }

        to = effectiveTo;

        var normalizedAccess = NormalizeAccess(access);
        var normalizedUsername =
            string.IsNullOrWhiteSpace(username)
                ? null
                : username.Trim();

        await using var dataSource =
            NpgsqlDataSource.Create(BuildConnectionString());

        var response = new AnalyticsResponse();

        response.Summary =
            await GetSummaryAsync(
                dataSource,
                from,
                to,
                normalizedUsername,
                normalizedAccess,
                cancellationToken);

        response.AccessBreakdown =
            await GetAccessBreakdownAsync(
                dataSource,
                from,
                to,
                normalizedUsername,
                normalizedAccess,
                cancellationToken);

        response.Daily =
            await GetDailyAsync(
                dataSource,
                from,
                to,
                normalizedUsername,
                normalizedAccess,
                cancellationToken);

        if (normalizedUsername == null)
        {
            response.TopUsers =
                await GetUsersAsync(
                    dataSource,
                    from,
                    to,
                    normalizedAccess,
                    "total_traffic",
                    10,
                    cancellationToken);

            response.TopDownloadUsers =
                await GetUsersAsync(
                    dataSource,
                    from,
                    to,
                    normalizedAccess,
                    "download",
                    5,
                    cancellationToken);

            response.TopUploadUsers =
                await GetUsersAsync(
                    dataSource,
                    from,
                    to,
                    normalizedAccess,
                    "upload",
                    5,
                    cancellationToken);

            response.TopDurationUsers =
                await GetUsersAsync(
                    dataSource,
                    from,
                    to,
                    normalizedAccess,
                    "duration",
                    5,
                    cancellationToken);

            response.TopOvpnUsers =
                await GetUsersAsync(
                    dataSource,
                    from,
                    to,
                    "OVPN",
                    "ovpn",
                    5,
                    cancellationToken);
        }
        else
        {
            response.TopUsers =
                await GetUsersAsync(
                    dataSource,
                    from,
                    to,
                    normalizedAccess,
                    "total_traffic",
                    1,
                    cancellationToken);
        }

        return response;
    }

    private async Task<AnalyticsSummary> GetSummaryAsync(
        NpgsqlDataSource dataSource,
        DateTimeOffset from,
        DateTimeOffset to,
        string? username,
        string access,
        CancellationToken cancellationToken)
    {
        const string sql = """
            WITH filtered AS (
                SELECT
                    username,
                    CASE
                        WHEN lower(coalesce(framedprotocol, '')) = 'ppp'
                          OR lower(coalesce(servicetype, '')) = 'framed-user'
                          OR lower(coalesce(nasporttype, '')) LIKE '%ppp%'
                        THEN 'OVPN'
                        ELSE 'Hotspot'
                    END AS access_type,
                    greatest(acctstarttime, @from) AS overlap_start,
                    least(coalesce(acctstoptime, @to), @to) AS overlap_end,
                    coalesce(acctoutputoctets, 0)::bigint AS download_bytes,
                    coalesce(acctinputoctets, 0)::bigint AS upload_bytes
                FROM public.radacct
                WHERE acctstarttime IS NOT NULL
                  AND acctstarttime < @to
                  AND coalesce(acctstoptime, @to) > @from
                  AND (@username IS NULL OR username ILIKE @username)
            ),
            scoped AS (
                SELECT *
                FROM filtered
                WHERE @access = 'all' OR access_type = @access
            )
            SELECT
                count(*)::bigint AS total_sessions,
                count(DISTINCT username)::int AS unique_users,
                coalesce(
                    sum(
                        greatest(
                            0,
                            extract(epoch FROM (overlap_end - overlap_start))
                        )
                    ),
                    0
                )::bigint AS total_duration_seconds,
                coalesce(sum(download_bytes), 0)::bigint AS total_download_bytes,
                coalesce(sum(upload_bytes), 0)::bigint AS total_upload_bytes,
                count(*) FILTER (WHERE access_type = 'OVPN')::bigint AS ovpn_sessions,
                coalesce(
                    sum(
                        greatest(
                            0,
                            extract(epoch FROM (overlap_end - overlap_start))
                        )
                    ) FILTER (WHERE access_type = 'OVPN'),
                    0
                )::bigint AS ovpn_duration_seconds,
                count(*) FILTER (WHERE access_type = 'Hotspot')::bigint AS hotspot_sessions,
                coalesce(
                    sum(
                        greatest(
                            0,
                            extract(epoch FROM (overlap_end - overlap_start))
                        )
                    ) FILTER (WHERE access_type = 'Hotspot'),
                    0
                )::bigint AS hotspot_duration_seconds
            FROM scoped;
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        AddCommonParameters(command, from, to, username, access);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return new AnalyticsSummary();
        }

        return new AnalyticsSummary
        {
            TotalSessions = reader.GetInt64(0),
            UniqueUsers = reader.GetInt32(1),
            TotalDurationSeconds = reader.GetInt64(2),
            TotalDownloadBytes = reader.GetInt64(3),
            TotalUploadBytes = reader.GetInt64(4),
            OvpnSessions = reader.GetInt64(5),
            OvpnDurationSeconds = reader.GetInt64(6),
            HotspotSessions = reader.GetInt64(7),
            HotspotDurationSeconds = reader.GetInt64(8)
        };
    }

    private async Task<List<AnalyticsBreakdown>> GetAccessBreakdownAsync(
        NpgsqlDataSource dataSource,
        DateTimeOffset from,
        DateTimeOffset to,
        string? username,
        string access,
        CancellationToken cancellationToken)
    {
        const string sql = """
            WITH filtered AS (
                SELECT
                    CASE
                        WHEN lower(coalesce(framedprotocol, '')) = 'ppp'
                          OR lower(coalesce(servicetype, '')) = 'framed-user'
                          OR lower(coalesce(nasporttype, '')) LIKE '%ppp%'
                        THEN 'OVPN'
                        ELSE 'Hotspot'
                    END AS access_type,
                    greatest(acctstarttime, @from) AS overlap_start,
                    least(coalesce(acctstoptime, @to), @to) AS overlap_end,
                    coalesce(acctoutputoctets, 0)::bigint AS download_bytes,
                    coalesce(acctinputoctets, 0)::bigint AS upload_bytes
                FROM public.radacct
                WHERE acctstarttime IS NOT NULL
                  AND acctstarttime < @to
                  AND coalesce(acctstoptime, @to) > @from
                  AND (@username IS NULL OR username ILIKE @username)
            )
            SELECT
                access_type,
                count(*)::bigint,
                coalesce(
                    sum(
                        greatest(
                            0,
                            extract(epoch FROM (overlap_end - overlap_start))
                        )
                    ),
                    0
                )::bigint,
                coalesce(sum(download_bytes), 0)::bigint,
                coalesce(sum(upload_bytes), 0)::bigint
            FROM filtered
            WHERE @access = 'all' OR access_type = @access
            GROUP BY access_type
            ORDER BY access_type;
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        AddCommonParameters(command, from, to, username, access);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var result = new List<AnalyticsBreakdown>();

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new AnalyticsBreakdown
            {
                Access = reader.GetString(0),
                Sessions = reader.GetInt64(1),
                DurationSeconds = reader.GetInt64(2),
                DownloadBytes = reader.GetInt64(3),
                UploadBytes = reader.GetInt64(4)
            });
        }

        return result;
    }

    private async Task<List<AnalyticsDailyPoint>> GetDailyAsync(
        NpgsqlDataSource dataSource,
        DateTimeOffset from,
        DateTimeOffset to,
        string? username,
        string access,
        CancellationToken cancellationToken)
    {
        const string sql = """
            WITH days AS (
                SELECT generate_series(
                    date_trunc('day', @from),
                    date_trunc('day', @to - interval '1 second'),
                    interval '1 day'
                ) AS day
            ),
            sessions AS (
                SELECT
                    username,
                    CASE
                        WHEN lower(coalesce(framedprotocol, '')) = 'ppp'
                          OR lower(coalesce(servicetype, '')) = 'framed-user'
                          OR lower(coalesce(nasporttype, '')) LIKE '%ppp%'
                        THEN 'OVPN'
                        ELSE 'Hotspot'
                    END AS access_type,
                    acctstarttime,
                    coalesce(acctstoptime, @to) AS acctendtime
                FROM public.radacct
                WHERE acctstarttime IS NOT NULL
                  AND acctstarttime < @to
                  AND coalesce(acctstoptime, @to) > @from
                  AND (@username IS NULL OR username ILIKE @username)
            )
            SELECT
                days.day,
                count(sessions.username)::bigint AS sessions,
                coalesce(
                    sum(
                        greatest(
                            0,
                            extract(
                                epoch FROM (
                                    least(
                                        sessions.acctendtime,
                                        days.day + interval '1 day'
                                    )
                                    -
                                    greatest(
                                        sessions.acctstarttime,
                                        days.day
                                    )
                                )
                            )
                        )
                    ),
                    0
                )::bigint AS duration_seconds
            FROM days
            LEFT JOIN sessions
                ON sessions.acctstarttime < days.day + interval '1 day'
               AND sessions.acctendtime > days.day
               AND (@access = 'all' OR sessions.access_type = @access)
            GROUP BY days.day
            ORDER BY days.day;
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(
            "from",
            NpgsqlDbType.TimestampTz,
            from);

        command.Parameters.AddWithValue(
            "to",
            NpgsqlDbType.TimestampTz,
            to);

        command.Parameters.AddWithValue(
            "username",
            NpgsqlDbType.Text,
            username == null
                ? DBNull.Value
                : $"%{username}%");

        command.Parameters.AddWithValue(
            "access",
            NpgsqlDbType.Text,
            access);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var result = new List<AnalyticsDailyPoint>();

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new AnalyticsDailyPoint
            {
                Date = reader.GetFieldValue<DateTimeOffset>(0),
                Sessions = reader.GetInt64(1),
                DurationSeconds = reader.GetInt64(2)
            });
        }

        return result;
    }

    public async Task<List<string>> SearchUsersAsync(
        string? query,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var normalizedQuery =
            string.IsNullOrWhiteSpace(query)
                ? null
                : query.Trim();

        if (normalizedQuery != null && normalizedQuery.Length < 2)
        {
            return [];
        }

        limit = Math.Clamp(limit, 1, 20);

        await using var dataSource =
            NpgsqlDataSource.Create(BuildConnectionString());

        const string sql = """
            SELECT username
            FROM public.radacct
            WHERE username IS NOT NULL
              AND btrim(username) <> ''
              AND (
                    @query IS NULL
                    OR username ILIKE @query
                  )
            GROUP BY username
            ORDER BY
                CASE
                    WHEN @plain_query IS NOT NULL
                         AND lower(username) = lower(@plain_query)
                    THEN 0
                    WHEN @plain_query IS NOT NULL
                         AND lower(username) LIKE lower(@plain_query) || '%'
                    THEN 1
                    ELSE 2
                END,
                lower(username)
            LIMIT @limit;
            """;

        await using var command = dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(
            "query",
            NpgsqlDbType.Text,
            normalizedQuery == null
                ? DBNull.Value
                : $"%{normalizedQuery}%");

        command.Parameters.AddWithValue(
            "plain_query",
            NpgsqlDbType.Text,
            normalizedQuery == null
                ? DBNull.Value
                : normalizedQuery);

        command.Parameters.AddWithValue(
            "limit",
            NpgsqlDbType.Integer,
            limit);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var result = new List<string>();

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(reader.GetString(0));
        }

        return result;
    }

    private async Task<List<AnalyticsUser>> GetUsersAsync(
        NpgsqlDataSource dataSource,
        DateTimeOffset from,
        DateTimeOffset to,
        string access,
        string orderBy,
        int limit,
        CancellationToken cancellationToken)
    {
        var orderClause = orderBy switch
        {
            "download" => "download_bytes DESC",
            "upload" => "upload_bytes DESC",
            "duration" => "duration_seconds DESC",
            "ovpn" => "ovpn_sessions DESC, ovpn_duration_seconds DESC",
            _ => "(download_bytes + upload_bytes) DESC"
        };

        var sql = $"""
            WITH filtered AS (
                SELECT
                    username,
                    CASE
                        WHEN lower(coalesce(framedprotocol, '')) = 'ppp'
                          OR lower(coalesce(servicetype, '')) = 'framed-user'
                          OR lower(coalesce(nasporttype, '')) LIKE '%ppp%'
                        THEN 'OVPN'
                        ELSE 'Hotspot'
                    END AS access_type,
                    greatest(acctstarttime, @from) AS overlap_start,
                    least(coalesce(acctstoptime, @to), @to) AS overlap_end,
                    coalesce(acctoutputoctets, 0)::bigint AS download_bytes,
                    coalesce(acctinputoctets, 0)::bigint AS upload_bytes
                FROM public.radacct
                WHERE acctstarttime IS NOT NULL
                  AND acctstarttime < @to
                  AND coalesce(acctstoptime, @to) > @from
            ),
            grouped AS (
                SELECT
                    username,
                    count(*)::bigint AS sessions,
                    coalesce(
                        sum(
                            greatest(
                                0,
                                extract(epoch FROM (overlap_end - overlap_start))
                            )
                        ),
                        0
                    )::bigint AS duration_seconds,
                    coalesce(sum(download_bytes), 0)::bigint AS download_bytes,
                    coalesce(sum(upload_bytes), 0)::bigint AS upload_bytes,
                    count(*) FILTER (WHERE access_type = 'OVPN')::bigint AS ovpn_sessions,
                    coalesce(
                        sum(
                            greatest(
                                0,
                                extract(epoch FROM (overlap_end - overlap_start))
                            )
                        ) FILTER (WHERE access_type = 'OVPN'),
                        0
                    )::bigint AS ovpn_duration_seconds,
                    count(*) FILTER (WHERE access_type = 'Hotspot')::bigint AS hotspot_sessions,
                    coalesce(
                        sum(
                            greatest(
                                0,
                                extract(epoch FROM (overlap_end - overlap_start))
                            )
                        ) FILTER (WHERE access_type = 'Hotspot'),
                        0
                    )::bigint AS hotspot_duration_seconds
                FROM filtered
                WHERE @access = 'all' OR access_type = @access
                GROUP BY username
            )
            SELECT
                username,
                sessions,
                duration_seconds,
                download_bytes,
                upload_bytes,
                ovpn_sessions,
                ovpn_duration_seconds,
                hotspot_sessions,
                hotspot_duration_seconds
            FROM grouped
            ORDER BY {orderClause}
            LIMIT @limit;
            """;

        await using var command =
            dataSource.CreateCommand(sql);

        command.Parameters.AddWithValue(
            "from",
            NpgsqlDbType.TimestampTz,
            from);

        command.Parameters.AddWithValue(
            "to",
            NpgsqlDbType.TimestampTz,
            to);

        command.Parameters.AddWithValue(
            "access",
            NpgsqlDbType.Text,
            access);

        command.Parameters.AddWithValue(
            "limit",
            NpgsqlDbType.Integer,
            limit);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var result = new List<AnalyticsUser>();

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new AnalyticsUser
            {
                Username = reader.GetString(0),
                Sessions = reader.GetInt64(1),
                DurationSeconds = reader.GetInt64(2),
                DownloadBytes = reader.GetInt64(3),
                UploadBytes = reader.GetInt64(4),
                OvpnSessions = reader.GetInt64(5),
                OvpnDurationSeconds = reader.GetInt64(6),
                HotspotSessions = reader.GetInt64(7),
                HotspotDurationSeconds = reader.GetInt64(8)
            });
        }

        return result;
    }

    private string BuildConnectionString()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = _options.Host,
            Port = _options.Port,
            Database = _options.Database,
            Username = _options.Username,
            Password = _options.Password,
            SslMode = SslMode.Disable,
            Timeout = 10,
            CommandTimeout = 20,
            ApplicationName = "BDIP Analytics"
        };

        return builder.ConnectionString;
    }

    private static void AddCommonParameters(
        NpgsqlCommand command,
        DateTimeOffset from,
        DateTimeOffset to,
        string? username,
        string access)
    {
        command.Parameters.AddWithValue(
            "from",
            NpgsqlDbType.TimestampTz,
            from);

        command.Parameters.AddWithValue(
            "to",
            NpgsqlDbType.TimestampTz,
            to);

        command.Parameters.AddWithValue(
            "username",
            NpgsqlDbType.Text,
            username == null
                ? DBNull.Value
                : $"%{username}%");

        command.Parameters.AddWithValue(
            "access",
            NpgsqlDbType.Text,
            access);
    }

    private static string NormalizeAccess(string? access)
    {
        if (string.Equals(access, "ovpn", StringComparison.OrdinalIgnoreCase))
        {
            return "OVPN";
        }

        if (string.Equals(access, "hotspot", StringComparison.OrdinalIgnoreCase))
        {
            return "Hotspot";
        }

        return "all";
    }
}
