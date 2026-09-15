using BDIP.Application.Audit;
using BDIP.Contracts.Audit;
using BDIP.Persistence.PostgreSQL;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BDIP.Infrastructure.Audit;

public sealed class PostgreSqlAuditLogService : IAuditLogService
{
    private readonly ApplicationDbOptions _options;

    public PostgreSqlAuditLogService(
        IOptions<ApplicationDbOptions> options)
    {
        _options = options.Value;
    }

    private NpgsqlDataSource CreateDataSource()
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
            CommandTimeout = 15,
            ApplicationName = "BDIP Backend"
        };

        return NpgsqlDataSource.Create(builder.ConnectionString);
    }

    public async Task<AuditLogListResponse> GetLogsAsync(
        AuditLogQuery query,
        CancellationToken cancellationToken = default)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 10, 200);
        var offset = (page - 1) * pageSize;

        var conditions = new List<string>();
        var parameters = new List<NpgsqlParameter>();

        if (!string.IsNullOrWhiteSpace(query.Username))
        {
            conditions.Add("username ILIKE @username");
            parameters.Add(new NpgsqlParameter("username", $"%{query.Username.Trim()}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.Module))
        {
            conditions.Add("module = @module");
            parameters.Add(new NpgsqlParameter("module", query.Module.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            conditions.Add("action = @action");
            parameters.Add(new NpgsqlParameter("action", query.Action.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(query.Result))
        {
            conditions.Add("result = @result");
            parameters.Add(new NpgsqlParameter(
                "result",
                query.Result.Trim().ToUpperInvariant()));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            conditions.Add(
                """
                (
                    username ILIKE @search
                    OR full_name ILIKE @search
                    OR action ILIKE @search
                    OR module ILIKE @search
                    OR target ILIKE @search
                    OR details ILIKE @search
                )
                """);

            parameters.Add(new NpgsqlParameter(
                "search",
                $"%{query.Search.Trim()}%"));
        }

        if (query.From.HasValue)
        {
            conditions.Add("created_at >= @from");
            parameters.Add(new NpgsqlParameter(
                "from",
                query.From.Value.UtcDateTime));
        }

        if (query.To.HasValue)
        {
            conditions.Add("created_at <= @to");
            parameters.Add(new NpgsqlParameter(
                "to",
                query.To.Value.UtcDateTime));
        }

        var whereClause = conditions.Count > 0
            ? "WHERE " + string.Join("\nAND ", conditions)
            : string.Empty;

        var result = new AuditLogListResponse
        {
            Page = page,
            PageSize = pageSize
        };

        await using var dataSource = CreateDataSource();

        var countSql = $"""
            SELECT COUNT(*)
            FROM public.audit_logs
            {whereClause};
            """;

        await using var countCommand = dataSource.CreateCommand(countSql);

        foreach (var parameter in parameters)
        {
            countCommand.Parameters.AddWithValue(
                parameter.ParameterName,
                parameter.Value);
        }

        result.Total = Convert.ToInt32(
            await countCommand.ExecuteScalarAsync(cancellationToken));

        result.TotalPages = result.Total == 0
            ? 0
            : (int)Math.Ceiling(
                result.Total / (double)pageSize);

        var dataSql = $"""
            SELECT
                id,
                created_at,
                username,
                full_name,
                role,
                action,
                module,
                target,
                result,
                ip_address,
                user_agent,
                details
            FROM public.audit_logs
            {whereClause}
            ORDER BY created_at DESC
            LIMIT @page_size
            OFFSET @offset;
            """;

        await using var command = dataSource.CreateCommand(dataSql);

        foreach (var parameter in parameters)
        {
            command.Parameters.AddWithValue(
                parameter.ParameterName,
                parameter.Value);
        }

        command.Parameters.AddWithValue("page_size", pageSize);
        command.Parameters.AddWithValue("offset", offset);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Logs.Add(new AuditLogResponse
            {
                Id = reader.GetGuid(0),
                CreatedAt = reader.GetFieldValue<DateTimeOffset>(1),
                Username = reader.IsDBNull(2) ? null : reader.GetString(2),
                FullName = reader.IsDBNull(3) ? null : reader.GetString(3),
                Role = reader.IsDBNull(4) ? null : reader.GetString(4),
                Action = reader.GetString(5),
                Module = reader.GetString(6),
                Target = reader.IsDBNull(7) ? null : reader.GetString(7),
                Result = reader.GetString(8),
                IpAddress = reader.IsDBNull(9) ? null : reader.GetString(9),
                UserAgent = reader.IsDBNull(10) ? null : reader.GetString(10),
                Details = reader.IsDBNull(11) ? null : reader.GetString(11)
            });
        }

        return result;
    }

    public async Task WriteAsync(
        string action,
        string module,
        string? username = null,
        string? fullName = null,
        string? role = null,
        string? target = null,
        string result = "SUCCESS",
        string? ipAddress = null,
        string? userAgent = null,
        string? details = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException(
                "Audit action is required.",
                nameof(action));

        if (string.IsNullOrWhiteSpace(module))
            throw new ArgumentException(
                "Audit module is required.",
                nameof(module));

        if (string.IsNullOrWhiteSpace(result))
            result = "SUCCESS";

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                INSERT INTO public.audit_logs
                (
                    username,
                    full_name,
                    role,
                    action,
                    module,
                    target,
                    result,
                    ip_address,
                    user_agent,
                    details
                )
                VALUES
                (
                    @username,
                    @fullname,
                    @role,
                    @action,
                    @module,
                    @target,
                    @result,
                    @ipaddress,
                    @useragent,
                    @details
                );
                """);

        command.Parameters.AddWithValue(
            "username",
            string.IsNullOrWhiteSpace(username)
                ? DBNull.Value
                : username);

        command.Parameters.AddWithValue(
            "fullname",
            string.IsNullOrWhiteSpace(fullName)
                ? DBNull.Value
                : fullName);

        command.Parameters.AddWithValue(
            "role",
            string.IsNullOrWhiteSpace(role)
                ? DBNull.Value
                : role);

        command.Parameters.AddWithValue(
            "action",
            action);

        command.Parameters.AddWithValue(
            "module",
            module);

        command.Parameters.AddWithValue(
            "target",
            string.IsNullOrWhiteSpace(target)
                ? DBNull.Value
                : target);

        command.Parameters.AddWithValue(
            "result",
            result);

        command.Parameters.AddWithValue(
            "ipaddress",
            string.IsNullOrWhiteSpace(ipAddress)
                ? DBNull.Value
                : ipAddress);

        command.Parameters.AddWithValue(
            "useragent",
            string.IsNullOrWhiteSpace(userAgent)
                ? DBNull.Value
                : userAgent);

        command.Parameters.AddWithValue(
            "details",
            string.IsNullOrWhiteSpace(details)
                ? DBNull.Value
                : details);

        await command.ExecuteNonQueryAsync();
    }
}
