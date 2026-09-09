using BDIP.Application.FingerMachines;
using BDIP.Contracts.FingerMachines;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.FingerMachines;

public sealed class PostgreSqlFingerMachineService
    : IFingerMachineService
{
    private readonly ApplicationDbOptions _options;

    public PostgreSqlFingerMachineService(
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

        return NpgsqlDataSource.Create(
            builder.ConnectionString);
    }

    public async Task<List<FingerMachineResponse>> GetAllAsync()
    {
        var result = new List<FingerMachineResponse>();

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    fm.id,
                    fm.code,
                    fm.name,
                    fm.ip_address,
                    fm.port,
                    fm.location_id,
                    COALESCE(l.name, '') AS location_name,
                    COALESCE(fm.serial_number, '') AS serial_number,
                    COALESCE(fm.device_name, '') AS device_name,
                    fm.is_active,
                    fm.created_at,
                    fm.updated_at
                FROM public.finger_machines fm
                LEFT JOIN public.locations l
                    ON l.id = fm.location_id
                ORDER BY fm.code;
                """);

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public async Task<FingerMachineResponse?> GetByCodeAsync(
        string code)
    {
        var normalizedCode =
            NormalizeCode(code);

        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            throw new InvalidOperationException(
                "Machine code is required.");
        }

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    fm.id,
                    fm.code,
                    fm.name,
                    fm.ip_address,
                    fm.port,
                    fm.location_id,
                    COALESCE(l.name, '') AS location_name,
                    COALESCE(fm.serial_number, '') AS serial_number,
                    COALESCE(fm.device_name, '') AS device_name,
                    fm.is_active,
                    fm.created_at,
                    fm.updated_at
                FROM public.finger_machines fm
                LEFT JOIN public.locations l
                    ON l.id = fm.location_id
                WHERE LOWER(fm.code) = LOWER(@code);
                """);

        command.Parameters.AddWithValue(
            "code",
            normalizedCode);

        await using var reader =
            await command.ExecuteReaderAsync();

        return await reader.ReadAsync()
            ? Map(reader)
            : null;
    }

    public async Task<FingerMachineResponse> CreateAsync(
        CreateFingerMachineRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var code =
            NormalizeCode(request.Code)
                .ToUpperInvariant();

        var name =
            NormalizeText(request.Name);

        var ip =
            NormalizeText(request.IpAddress);

        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException(
                "Machine code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException(
                "Machine name is required.");

        if (string.IsNullOrWhiteSpace(ip))
            throw new InvalidOperationException(
                "IP address is required.");

        ValidatePort(request.Port);

        await using var dataSource = CreateDataSource();

        await EnsureLocationExistsAsync(
            dataSource,
            request.LocationId);

        await using var duplicateCommand =
            dataSource.CreateCommand(
                """
                SELECT 1
                FROM public.finger_machines
                WHERE LOWER(code) = LOWER(@code);
                """);

        duplicateCommand.Parameters.AddWithValue(
            "code",
            code);

        await using var duplicateReader =
            await duplicateCommand.ExecuteReaderAsync();

        if (await duplicateReader.ReadAsync())
        {
            throw new InvalidOperationException(
                $"Machine code '{code}' already exists.");
        }

        var id = Guid.NewGuid();
        var now = DateTime.UtcNow;

        await using var insertCommand =
            dataSource.CreateCommand(
                """
                INSERT INTO public.finger_machines
                (
                    id,
                    code,
                    name,
                    ip_address,
                    port,
                    location_id,
                    is_active,
                    created_at,
                    updated_at
                )
                VALUES
                (
                    @id,
                    @code,
                    @name,
                    @ip_address,
                    @port,
                    @location_id,
                    TRUE,
                    @created_at,
                    @updated_at
                );
                """);

        insertCommand.Parameters.AddWithValue("id", id);
        insertCommand.Parameters.AddWithValue("code", code);
        insertCommand.Parameters.AddWithValue("name", name);
        insertCommand.Parameters.AddWithValue("ip_address", ip);
        insertCommand.Parameters.AddWithValue(
            "port",
            request.Port);

        insertCommand.Parameters.AddWithValue(
            "location_id",
            (object?)request.LocationId
                ?? DBNull.Value);

        insertCommand.Parameters.AddWithValue(
            "created_at",
            now);

        insertCommand.Parameters.AddWithValue(
            "updated_at",
            now);

        await insertCommand.ExecuteNonQueryAsync();

        return await GetByCodeAsync(code)
            ?? throw new InvalidOperationException(
                "Machine was not found after create.");
    }

    public async Task<FingerMachineResponse> UpdateAsync(
        string code,
        UpdateFingerMachineRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedCode =
            NormalizeCode(code);

        var name =
            NormalizeText(request.Name);

        var ip =
            NormalizeText(request.IpAddress);

        if (string.IsNullOrWhiteSpace(normalizedCode))
            throw new InvalidOperationException(
                "Machine code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException(
                "Machine name is required.");

        if (string.IsNullOrWhiteSpace(ip))
            throw new InvalidOperationException(
                "IP address is required.");

        ValidatePort(request.Port);

        await using var dataSource = CreateDataSource();

        await EnsureLocationExistsAsync(
            dataSource,
            request.LocationId);

        await using var command =
            dataSource.CreateCommand(
                """
                UPDATE public.finger_machines
                SET
                    name = @name,
                    ip_address = @ip_address,
                    port = @port,
                    location_id = @location_id,
                    serial_number = NULLIF(@serial_number, ''),
                    device_name = NULLIF(@device_name, ''),
                    is_active = @is_active,
                    updated_at = @updated_at
                WHERE LOWER(code) = LOWER(@code);
                """);

        command.Parameters.AddWithValue(
            "code",
            normalizedCode);

        command.Parameters.AddWithValue(
            "name",
            name);

        command.Parameters.AddWithValue(
            "ip_address",
            ip);

        command.Parameters.AddWithValue(
            "port",
            request.Port);

        command.Parameters.AddWithValue(
            "location_id",
            (object?)request.LocationId
                ?? DBNull.Value);

        command.Parameters.AddWithValue(
            "serial_number",
            NormalizeText(request.SerialNumber));

        command.Parameters.AddWithValue(
            "device_name",
            NormalizeText(request.DeviceName));

        command.Parameters.AddWithValue(
            "is_active",
            request.IsActive);

        command.Parameters.AddWithValue(
            "updated_at",
            DateTime.UtcNow);

        var affected =
            await command.ExecuteNonQueryAsync();

        if (affected == 0)
        {
            throw new InvalidOperationException(
                $"Machine '{normalizedCode}' not found.");
        }

        return await GetByCodeAsync(normalizedCode)
            ?? throw new InvalidOperationException(
                "Machine was not found after update.");
    }

    public async Task DeactivateAsync(string code)
    {
        var normalizedCode =
            NormalizeCode(code);

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                UPDATE public.finger_machines
                SET
                    is_active = FALSE,
                    updated_at = @updated_at
                WHERE LOWER(code) = LOWER(@code);
                """);

        command.Parameters.AddWithValue(
            "code",
            normalizedCode);

        command.Parameters.AddWithValue(
            "updated_at",
            DateTime.UtcNow);

        var affected =
            await command.ExecuteNonQueryAsync();

        if (affected == 0)
        {
            throw new InvalidOperationException(
                $"Machine '{normalizedCode}' not found.");
        }
    }

    public async Task DeleteAsync(string code)
    {
        var normalizedCode =
            NormalizeCode(code);

        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            throw new InvalidOperationException(
                "Machine code is required.");
        }

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                DELETE FROM public.finger_machines
                WHERE LOWER(code) = LOWER(@code);
                """
            );

        command.Parameters.AddWithValue(
            "code",
            normalizedCode);

        var affected =
            await command.ExecuteNonQueryAsync();

        if (affected == 0)
        {
            throw new InvalidOperationException(
                $"Machine '{normalizedCode}' not found.");
        }
    }

    private static async Task EnsureLocationExistsAsync(
        NpgsqlDataSource dataSource,
        Guid? locationId)
    {
        if (locationId is null)
            return;

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT 1
                FROM public.locations
                WHERE id = @id;
                """);

        command.Parameters.AddWithValue(
            "id",
            locationId.Value);

        var exists =
            await command.ExecuteScalarAsync();

        if (exists is null)
        {
            throw new InvalidOperationException(
                "Selected location was not found.");
        }
    }

    private static FingerMachineResponse Map(
        NpgsqlDataReader reader)
    {
        return new FingerMachineResponse
        {
            Id = reader.GetGuid(0),
            Code = reader.GetString(1),
            Name = reader.GetString(2),
            IpAddress = reader.GetString(3),
            Port = reader.GetInt32(4),
            LocationId =
                reader.IsDBNull(5)
                    ? null
                    : reader.GetGuid(5),
            LocationName = reader.GetString(6),
            SerialNumber = reader.GetString(7),
            DeviceName = reader.GetString(8),
            IsActive = reader.GetBoolean(9),
            CreatedAt = reader.GetDateTime(10),
            UpdatedAt = reader.GetDateTime(11)
        };
    }

    private static string NormalizeCode(string value)
        => value.Trim();

    private static string NormalizeText(string? value)
        => value?.Trim() ?? "";

    private static void ValidatePort(int port)
    {
        if (port <= 0 || port > 65535)
        {
            throw new InvalidOperationException(
                "Port must be between 1 and 65535.");
        }
    }
}
