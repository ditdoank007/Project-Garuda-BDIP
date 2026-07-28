using BDIP.Application.NAP;
using BDIP.Domain.NAP;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.NAP;

public sealed class PostgreSqlPolicyService : IPolicyService
{
    private readonly ApplicationDbOptions _options;

    public PostgreSqlPolicyService(
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

    public async Task<IEnumerable<Policy>> GetAllAsync()
    {
        var result = new List<Policy>();

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    id,
                    code,
                    name,
                    description,
                    is_active,
                    created_at,
                    updated_at
                FROM policies
                ORDER BY code;
                """);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public async Task<Policy?> GetByIdAsync(Guid id)
    {
        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    id,
                    code,
                    name,
                    description,
                    is_active,
                    created_at,
                    updated_at
                FROM policies
                WHERE id = @id;
                """);

        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
            return Map(reader);

        return null;
    }

    public async Task<Policy?> GetByCodeAsync(string code)
    {
        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    id,
                    code,
                    name,
                    description,
                    is_active,
                    created_at,
                    updated_at
                FROM policies
                WHERE LOWER(code)=LOWER(@code);
                """);

        command.Parameters.AddWithValue("code", code);

        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
            return Map(reader);

        return null;
    }

    public async Task<Policy> CreateAsync(Policy policy)
    {
        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                INSERT INTO policies
                (
                    id,
                    code,
                    name,
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
                    @description,
                    @is_active,
                    @created_at,
                    @updated_at
                );
                """);

        command.Parameters.AddWithValue("id", policy.Id);
        command.Parameters.AddWithValue("code", policy.Code);
        command.Parameters.AddWithValue("name", policy.Name);
        command.Parameters.AddWithValue("description",
            (object?)policy.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("is_active", policy.IsActive);
        command.Parameters.AddWithValue("created_at", policy.CreatedAt);
        command.Parameters.AddWithValue("updated_at", policy.UpdatedAt);

        await command.ExecuteNonQueryAsync();

        return policy;
    }

    public async Task<Policy> UpdateAsync(Policy policy)
    {
        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                UPDATE policies
                SET
                    code=@code,
                    name=@name,
                    description=@description,
                    is_active=@is_active,
                    updated_at=@updated_at
                WHERE id=@id;
                """);

        command.Parameters.AddWithValue("id", policy.Id);
        command.Parameters.AddWithValue("code", policy.Code);
        command.Parameters.AddWithValue("name", policy.Name);
        command.Parameters.AddWithValue("description",
            (object?)policy.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("is_active", policy.IsActive);
        policy.UpdatedAt = DateTime.UtcNow;
        command.Parameters.AddWithValue("updated_at", DateTime.UtcNow);

        var affected =
            await command.ExecuteNonQueryAsync();

        if (affected == 0)
        {
            throw new KeyNotFoundException(
                $"Policy '{policy.Id}' not found.");
        }

        policy.UpdatedAt = DateTime.UtcNow;

        return policy;
    }

    public async Task DeleteAsync(Guid id)
    {
        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                DELETE FROM policies
                WHERE id=@id;
                """);

        command.Parameters.AddWithValue("id", id);

        var affected =
            await command.ExecuteNonQueryAsync();

        if (affected == 0)
        {
            throw new KeyNotFoundException(
                $"Policy '{id}' not found.");
        }
    }

    private static Policy Map(NpgsqlDataReader reader)
    {
        return new Policy
        {
            Id = reader.GetGuid(0),
            Code = reader.GetString(1),
            Name = reader.GetString(2),
            Description =
                reader.IsDBNull(3)
                    ? null
                    : reader.GetString(3),
            IsActive = reader.GetBoolean(4),
            CreatedAt = reader.GetFieldValue<DateTime>(5),
            UpdatedAt = reader.GetFieldValue<DateTime>(6)
        };
    }
}