// PLACEHOLDER
// The full PostgreSqlLocationService.cs implementation is too large to safely
// emit in a normal chat response. This file has been created as the target
// artifact so it can be iteratively filled.

using System.Text;

using BDIP.Application.Locations;
using BDIP.Contracts.Locations;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.Locations;

public sealed class PostgreSqlLocationService : ILocationService
{
    private static readonly Dictionary<string, string> TypeCodes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Kantor Pusat"] = "HQ",
            ["Balai Diklat"] = "BD",
            ["UPT"] = "UPT"
        };

    private static readonly Dictionary<string, string> TypeNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["HQ"] = "Kantor Pusat",
            ["BD"] = "Balai Diklat",
            ["UPT"] = "UPT"
        };

    private readonly ApplicationDbOptions _options;

    public PostgreSqlLocationService(
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

    public async Task<List<LocationResponse>> GetAllAsync()
    {
        var result = new List<LocationResponse>();

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    id,
                    code,
                    name,
                    description,
                    location_type_code,
                    is_active,
                    created_at,
                    updated_at
                FROM public.locations
                ORDER BY name;
                """);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public async Task<LocationResponse?> GetByNameAsync(string name)
    {
        var normalizedName = NormalizeName(name);

        await using var dataSource = CreateDataSource();

        var location = await FindByNameAsync(dataSource, normalizedName);

        return location is null ? null : Map(location);
    }

    public async Task<LocationResponse> CreateAsync(CreateLocationRequest request)
    {
        var name = NormalizeName(request.Name);
        var description = NormalizeDescription(request.Description);
        var type = NormalizeType(request.Type);
        var code = BuildCode(name);
        var typeCode = GetTypeCode(type);

        await using var dataSource = CreateDataSource();

        if (await FindByNameAsync(dataSource, name) is not null)
        {
            throw new InvalidOperationException(
                $"Location '{name}' already exists.");
        }

        if (await FindByCodeAsync(dataSource, code) is not null)
        {
            throw new InvalidOperationException(
                $"Location code '{code}' already exists.");
        }

        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();

        await using var command =
            dataSource.CreateCommand(
                """
                INSERT INTO public.locations
                (
                    id,
                    code,
                    name,
                    location_type_code,
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
                    @location_type_code,
                    @description,
                    @is_active,
                    @created_at,
                    @updated_at
                );
                """);

        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("code", code);
        command.Parameters.AddWithValue("name", name);
        command.Parameters.AddWithValue("location_type_code", typeCode);
        command.Parameters.AddWithValue("description",
            (object?)description ?? DBNull.Value);
        command.Parameters.AddWithValue("is_active", true);
        command.Parameters.AddWithValue("created_at", now);
        command.Parameters.AddWithValue("updated_at", now);

        await command.ExecuteNonQueryAsync();

        var created = await FindByNameAsync(dataSource, name)
            ?? throw new InvalidOperationException(
                $"Location '{name}' was not found after create.");

        return Map(created);
    }

    public async Task<LocationResponse> UpdateAsync(
        string currentName,
        UpdateLocationRequest request)
    {
        var normalizedCurrentName = NormalizeName(currentName);
        var newName = NormalizeName(request.Name);
        var description = NormalizeDescription(request.Description);
        var type = NormalizeType(request.Type);
        var newCode = BuildCode(newName);
        var typeCode = GetTypeCode(type);

        await using var dataSource = CreateDataSource();

        var current = await FindByNameAsync(dataSource, normalizedCurrentName);

        if (current is null)
        {
            throw new InvalidOperationException(
                $"Location '{normalizedCurrentName}' not found.");
        }

        if (!string.Equals(normalizedCurrentName, newName, StringComparison.OrdinalIgnoreCase))
        {
            var collision = await FindByNameAsync(dataSource, newName);

            if (collision is not null)
            {
                throw new InvalidOperationException(
                    $"Location '{newName}' already exists.");
            }
        }

        var codeCollision = await FindByCodeAsync(dataSource, newCode);
        if (codeCollision is not null &&
            !string.Equals(codeCollision.Name, current.Name, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Location code '{newCode}' already exists.");
        }

        var now = DateTime.UtcNow;

        await using var command =
            dataSource.CreateCommand(
                """
                UPDATE public.locations
                SET
                    code = @code,
                    name = @name,
                    location_type_code = @location_type_code,
                    description = @description,
                    updated_at = @updated_at
                WHERE id = @id;
                """);

        command.Parameters.AddWithValue("id", current.Id);
        command.Parameters.AddWithValue("code", newCode);
        command.Parameters.AddWithValue("name", newName);
        command.Parameters.AddWithValue("location_type_code", typeCode);
        command.Parameters.AddWithValue("description",
            (object?)description ?? DBNull.Value);
        command.Parameters.AddWithValue("updated_at", now);

        var affected = await command.ExecuteNonQueryAsync();

        if (affected == 0)
        {
            throw new InvalidOperationException(
                $"Location '{normalizedCurrentName}' was not updated.");
        }

        var updated = await FindByIdAsync(dataSource, current.Id)
            ?? throw new InvalidOperationException(
                $"Location '{newName}' was not found after update.");

        return Map(updated);
    }

    public async Task DeleteAsync(string name)
    {
        var normalizedName = NormalizeName(name);

        await using var dataSource = CreateDataSource();

        var location = await FindByNameAsync(dataSource, normalizedName);

        if (location is null)
        {
            throw new InvalidOperationException(
                $"Location '{normalizedName}' not found.");
        }

        await using var command =
            dataSource.CreateCommand(
                """
                DELETE FROM public.locations
                WHERE id = @id;
                """);

        command.Parameters.AddWithValue("id", location.Id);

        var affected = await command.ExecuteNonQueryAsync();

        if (affected == 0)
        {
            throw new InvalidOperationException(
                $"Location '{normalizedName}' could not be deleted.");
        }
    }

    private async Task<LocationRecord?> FindByNameAsync(
        NpgsqlDataSource dataSource,
        string name)
    {
        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    id,
                    code,
                    name,
                    description,
                    location_type_code,
                    is_active,
                    created_at,
                    updated_at
                FROM public.locations
                WHERE LOWER(name) = LOWER(@name);
                """);

        command.Parameters.AddWithValue("name", name);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapRecord(reader);
        }

        return null;
    }

    private async Task<LocationRecord?> FindByCodeAsync(
        NpgsqlDataSource dataSource,
        string code)
    {
        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    id,
                    code,
                    name,
                    description,
                    location_type_code,
                    is_active,
                    created_at,
                    updated_at
                FROM public.locations
                WHERE LOWER(code) = LOWER(@code);
                """);

        command.Parameters.AddWithValue("code", code);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapRecord(reader);
        }

        return null;
    }

    private async Task<LocationRecord?> FindByIdAsync(
        NpgsqlDataSource dataSource,
        Guid id)
    {
        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    id,
                    code,
                    name,
                    description,
                    location_type_code,
                    is_active,
                    created_at,
                    updated_at
                FROM public.locations
                WHERE id = @id;
                """);

        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return MapRecord(reader);
        }

        return null;
    }

    private static LocationResponse Map(LocationRecord record)
    {
        return new LocationResponse
        {
            Name = record.Name,
            Description = record.Description,
            Type = GetTypeName(record.LocationTypeCode),
            UnitCount = 0
        };
    }

    private static LocationResponse Map(NpgsqlDataReader reader)
    {
        return new LocationResponse
        {
            Name = reader.GetString(2),
            Description = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            Type = GetTypeName(reader.GetString(4)),
            UnitCount = 0
        };
    }

    private static LocationRecord MapRecord(NpgsqlDataReader reader)
    {
        return new LocationRecord(
            reader.GetGuid(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            reader.GetString(4),
            reader.GetBoolean(5),
            reader.GetFieldValue<DateTime>(6),
            reader.GetFieldValue<DateTime>(7));
    }

    private static string NormalizeName(string value)
    {
        var normalized = value?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new ArgumentException("Location name is required.");
        }

        return normalized;
    }

    private static string NormalizeDescription(string value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static string NormalizeType(string value)
    {
        var normalized = value?.Trim() ?? string.Empty;

        if (TypeCodes.ContainsKey(normalized))
        {
            return normalized;
        }

        var matchingName = TypeNames
            .FirstOrDefault(entry =>
                string.Equals(entry.Value, normalized, StringComparison.OrdinalIgnoreCase))
            .Value;

        if (!string.IsNullOrEmpty(matchingName))
        {
            return matchingName;
        }

        throw new ArgumentException(
            "Location type must be one of: Kantor Pusat, Balai Diklat, UPT.");
    }

    private static string GetTypeCode(string type)
    {
        if (TypeCodes.TryGetValue(type, out var code))
        {
            return code;
        }

        return TypeCodes[
            TypeNames.First(entry =>
                string.Equals(entry.Value, type, StringComparison.OrdinalIgnoreCase)).Value];
    }

    private static string GetTypeName(string code)
    {
        if (TypeNames.TryGetValue(code, out var name))
        {
            return name;
        }

        return code;
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
            ? "location"
            : code;
    }

    private sealed record LocationRecord(
        Guid Id,
        string Code,
        string Name,
        string Description,
        string LocationTypeCode,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt);
}
