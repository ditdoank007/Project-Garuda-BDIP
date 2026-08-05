using System.Text;

using BDIP.Application.Units;
using BDIP.Contracts.Units;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.Units;

public sealed class PostgreSqlUnitService : IUnitService
{
    private readonly ApplicationDbOptions _options;

    public PostgreSqlUnitService(
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

    public async Task<List<UnitResponse>> GetAllAsync()
    {
        var result = new List<UnitResponse>();

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    u.id,
                    u.code,
                    u.name,
                    u.description,
                    u.location_id,
                    u.is_active,
                    u.created_at,
                    u.updated_at,
                    l.name AS location_name
                FROM public.units AS u
                LEFT JOIN public.locations AS l
                    ON l.id = u.location_id
                ORDER BY u.name;
                """);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public async Task<UnitResponse?> GetByNameAsync(string name)
    {
        var normalizedName = NormalizeName(name);

        await using var dataSource = CreateDataSource();

        var unit = await FindByNameAsync(dataSource, normalizedName);

        return unit is null ? null : Map(unit);
    }

    public async Task<UnitResponse> CreateAsync(CreateUnitRequest request)
    {
        var name = NormalizeName(request.Name);
        var description = NormalizeDescription(request.Description);
        var code = BuildCode(name);

        await using var dataSource = CreateDataSource();

        if (await FindByNameAsync(dataSource, name) is not null)
        {
            throw new InvalidOperationException(
                $"Unit '{name}' already exists.");
        }

        if (await FindByCodeAsync(dataSource, code) is not null)
        {
            throw new InvalidOperationException(
                $"Unit code '{code}' already exists.");
        }

        if (!await LocationExistsAsync(dataSource, request.LocationId))
        {
            throw new InvalidOperationException("Location not found.");
        }

        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();

        await using var command =
            dataSource.CreateCommand(
                """
                INSERT INTO public.units
                (
                    id,
                    code,
                    name,
                    location_id,
                    description,
                    is_active,
                    created_at,
                    updated_at
                )
                VALUES
                (
                    @id,
                    @code,
                    @name,
                    @location_id,
                    @description,
                    @is_active,
                    @created_at,
                    @updated_at
                );
                """);

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("code", code);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("location_id", request.LocationId);
        command.Parameters.AddWithValue("description",
            (object?)description ?? DBNull.Value);
        command.Parameters.AddWithValue("is_active", true);
        command.Parameters.AddWithValue("created_at", now);
        command.Parameters.AddWithValue("updated_at", now);

        await command.ExecuteNonQueryAsync();

        var created = await FindByNameAsync(dataSource, name)
            ?? throw new InvalidOperationException(
                $"Unit '{name}' was not found after create.");

        return Map(created);
    }

    public async Task<UnitResponse> UpdateAsync(
        string currentName,
        UpdateUnitRequest request)
    {
        var normalizedCurrentName = NormalizeName(currentName);
        var newName = NormalizeName(request.Name);
        var description = NormalizeDescription(request.Description);
        var code = BuildCode(newName);

        await using var dataSource = CreateDataSource();

        var current = await FindByNameAsync(dataSource, normalizedCurrentName);

        if (current is null)
        {
            throw new InvalidOperationException(
                $"Unit '{normalizedCurrentName}' not found.");
        }

        if (!string.Equals(normalizedCurrentName, newName, StringComparison.OrdinalIgnoreCase))
        {
            var collision = await FindByNameAsync(dataSource, newName);

            if (collision is not null)
            {
                throw new InvalidOperationException(
                    $"Unit '{newName}' already exists.");
            }
        }

        var codeCollision = await FindByCodeAsync(dataSource, code);
        if (codeCollision is not null &&
            codeCollision.Id != current.Id)
        {
            throw new InvalidOperationException(
                $"Unit code '{code}' already exists.");
        }

        if (!await LocationExistsAsync(dataSource, request.LocationId))
        {
            throw new InvalidOperationException("Location not found.");
        }

        var now = DateTime.UtcNow;

        await using var command =
            dataSource.CreateCommand(
                """
                UPDATE public.units
                SET
                    code = @code,
                    name = @name,
                    location_id = @location_id,
                    description = @description,
                    is_active = @is_active,
                    updated_at = @updated_at
                WHERE id = @id;
                """);

        command.Parameters.AddWithValue("id", current.Id);
        command.Parameters.AddWithValue("code", code);
        command.Parameters.AddWithValue("name", newName);
        command.Parameters.AddWithValue("location_id", request.LocationId);
        command.Parameters.AddWithValue("description",
            (object?)description ?? DBNull.Value);
        command.Parameters.AddWithValue("is_active", request.IsActive);
        command.Parameters.AddWithValue("updated_at", now);

        var affected = await command.ExecuteNonQueryAsync();

        if (affected == 0)
        {
            throw new InvalidOperationException(
                $"Unit '{normalizedCurrentName}' was not updated.");
        }

        var updated = await FindByIdAsync(dataSource, current.Id)
            ?? throw new InvalidOperationException(
                $"Unit '{newName}' was not found after update.");

        return Map(updated);
    }

    public async Task DeleteAsync(string name)
    {
        var normalizedName = NormalizeName(name);

        await using var dataSource = CreateDataSource();

        var unit = await FindByNameAsync(dataSource, normalizedName);

        if (unit is null)
        {
            throw new InvalidOperationException(
                $"Unit '{normalizedName}' not found.");
        }

        await using var command =
            dataSource.CreateCommand(
                """
                DELETE FROM public.units
                WHERE id = @id;
                """);

        command.Parameters.AddWithValue("id", unit.Id);

        var affected = await command.ExecuteNonQueryAsync();

        if (affected == 0)
        {
            throw new InvalidOperationException(
                $"Unit '{normalizedName}' could not be deleted.");
        }

        
    }

    private async Task<UnitRecord?> FindByNameAsync(
        NpgsqlDataSource dataSource,
        string name)
    {
        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    u.id,
                    u.code,
                    u.name,
                    u.description,
                    u.location_id,
                    u.is_active,
                    u.created_at,
                    u.updated_at,
                    l.name AS location_name
                FROM public.units AS u
                LEFT JOIN public.locations AS l
                    ON l.id = u.location_id
                WHERE LOWER(u.name) = LOWER(@name);
                """);

        command.Parameters.AddWithValue("name", name);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapRecord(reader);
        }

        return null;
    }

    private async Task<UnitRecord?> FindByCodeAsync(
        NpgsqlDataSource dataSource,
        string code)
    {
        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    u.id,
                    u.code,
                    u.name,
                    u.description,
                    u.location_id,
                    u.is_active,
                    u.created_at,
                    u.updated_at,
                    l.name AS location_name
                FROM public.units AS u
                LEFT JOIN public.locations AS l
                    ON l.id = u.location_id
                WHERE LOWER(u.code) = LOWER(@code);
                """);

        command.Parameters.AddWithValue("code", code);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapRecord(reader);
        }

        return null;
    }

    private async Task<UnitRecord?> FindByIdAsync(
        NpgsqlDataSource dataSource,
        Guid id)
    {
        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    u.id,
                    u.code,
                    u.name,
                    u.description,
                    u.location_id,
                    u.is_active,
                    u.created_at,
                    u.updated_at,
                    l.name AS location_name
                FROM public.units AS u
                LEFT JOIN public.locations AS l
                    ON l.id = u.location_id
                WHERE u.id = @id;
                """);

        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapRecord(reader);
        }

        return null;
    }

    private async Task<bool> LocationExistsAsync(
        NpgsqlDataSource dataSource,
        Guid locationId)
    {
        await using var command =
            dataSource.CreateCommand(
                """
                SELECT 1
                FROM public.locations
                WHERE id = @id;
                """);

        command.Parameters.AddWithValue("id", locationId);

        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync();
    }

    private static UnitResponse Map(UnitRecord record)
    {
        return new UnitResponse
        {
            Id = record.Id,
            Code = record.Code,
            Name = record.Name,
            Description = record.Description,
            LocationId = record.LocationId,
            LocationName = record.LocationName,
            IsActive = record.IsActive
        };
    }

    private static UnitResponse Map(NpgsqlDataReader reader)
    {
        return new UnitResponse
        {
            Id = reader.GetGuid(0),
            Code = reader.GetString(1),
            Name = reader.GetString(2),
            Description = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            LocationId = reader.GetGuid(4),
            LocationName = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
            IsActive = reader.GetBoolean(5)
        };
    }

    private static UnitRecord MapRecord(NpgsqlDataReader reader)
    {
        return new UnitRecord(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            reader.GetGuid(4),
            reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
            reader.GetBoolean(5),
            reader.GetFieldValue<DateTime>(6),
            reader.GetFieldValue<DateTime>(7));
    }

    private static string NormalizeName(string value)
    {
        var normalized = value?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Unit name is required.");
        }

        return normalized;
    }

    private static string NormalizeDescription(string value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static string BuildCode(string name)
    {
        var builder = new StringBuilder();

        foreach (var character in name.Trim().ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
            }
            else if (builder.Length > 0 && builder[^1] != '-')
            {
                builder.Append('-');
            }
        }

        var code = builder.ToString().Trim('-');

        return string.IsNullOrWhiteSpace(code)
            ? "unit"
            : code;
    }

    private sealed record UnitRecord(
        Guid Id,
        string Code,
        string Name,
        string Description,
        Guid LocationId,
        string LocationName,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
