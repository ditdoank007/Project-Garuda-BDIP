using BDIP.Application.ExternalLogServer;
using BDIP.Contracts.ExternalLogServer;
using BDIP.Persistence.PostgreSQL;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BDIP.Infrastructure.ExternalLogServer;

public class PostgreSqlExternalLogServerConfigService
    : IExternalLogServerConfigService
{
    private readonly ApplicationDbOptions _dbOptions;

    public PostgreSqlExternalLogServerConfigService(
        IOptions<ApplicationDbOptions> options)
    {
        _dbOptions = options.Value;
    }

    private NpgsqlDataSource CreateDataSource()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = _dbOptions.Host,
            Port = _dbOptions.Port,
            Database = _dbOptions.Database,
            Username = _dbOptions.Username,
            Password = _dbOptions.Password,
            SslMode = SslMode.Disable,
            Timeout = 10,
            CommandTimeout = 15,
            ApplicationName = "BDIP Backend"
        };

        return NpgsqlDataSource.Create(builder.ConnectionString);
    }

    public async Task<ExternalLogServerConfigResponse> GetAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dataSource = CreateDataSource();

        await using var command = dataSource.CreateCommand(@"
            SELECT
                enabled,
                server_address,
                port,
                protocol,
                facility,
                updated_at
            FROM public.external_log_server_config
            WHERE id = 1;
        ");

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "External log server configuration was not found.");
        }

        return Map(reader);
    }

    public async Task<ExternalLogServerConfigResponse> UpdateAsync(
        ExternalLogServerConfigRequest request,
        CancellationToken cancellationToken = default)
    {
        var protocol = request.Protocol.Trim().ToUpperInvariant();
        var serverAddress = string.IsNullOrWhiteSpace(request.ServerAddress)
            ? null
            : request.ServerAddress.Trim();

        if (request.Enabled && string.IsNullOrWhiteSpace(serverAddress))
        {
            throw new ArgumentException(
                "Server address is required when external log server is enabled.");
        }

        if (request.Port is < 1 or > 65535)
        {
            throw new ArgumentException(
                "Port must be between 1 and 65535.");
        }

        if (protocol is not ("UDP" or "TCP"))
        {
            throw new ArgumentException(
                "Protocol must be UDP or TCP.");
        }

        if (request.Facility is < 0 or > 23)
        {
            throw new ArgumentException(
                "Facility must be between 0 and 23.");
        }

        await using var dataSource = CreateDataSource();

        await using var command = dataSource.CreateCommand(@"
            UPDATE public.external_log_server_config
            SET
                enabled = @enabled,
                server_address = @server_address,
                port = @port,
                protocol = @protocol,
                facility = @facility,
                updated_at = NOW()
            WHERE id = 1
            RETURNING
                enabled,
                server_address,
                port,
                protocol,
                facility,
                updated_at;
        ");

        command.Parameters.AddWithValue("enabled", request.Enabled);
        command.Parameters.AddWithValue(
            "server_address",
            (object?)serverAddress ?? DBNull.Value);
        command.Parameters.AddWithValue("port", request.Port);
        command.Parameters.AddWithValue("protocol", protocol);
        command.Parameters.AddWithValue("facility", request.Facility);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "External log server configuration was not found.");
        }

        return Map(reader);
    }

    private static ExternalLogServerConfigResponse Map(
        NpgsqlDataReader reader)
    {
        return new ExternalLogServerConfigResponse
        {
            Enabled = reader.GetBoolean(0),
            ServerAddress = reader.IsDBNull(1)
                ? null
                : reader.GetString(1),
            Port = reader.GetInt32(2),
            Protocol = reader.GetString(3),
            Facility = reader.GetInt32(4),
            UpdatedAt = reader.GetFieldValue<DateTimeOffset>(5)
        };
    }
}
