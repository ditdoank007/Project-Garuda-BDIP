using BDIP.Application.Applications;
using BDIP.Contracts.Applications;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.Applications;

public sealed class PostgreSqlApplicationService : IApplicationService
{
    private readonly ApplicationDbOptions _options;

    public PostgreSqlApplicationService(
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

    public async Task<List<ApplicationResponse>> GetAllAsync()
    {
        var result = new List<ApplicationResponse>();

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    id,
                    code,
                    name,
                    description,
                    base_url,
                    is_active,
                    created_at,
                    updated_at
                FROM public.applications
                ORDER BY name;
                """);

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(Map(reader));
        }

        return result;
    }

    public async Task<ApplicationResponse?> GetByCodeAsync(string code)
    {
        var normalizedCode = NormalizeCode(code);

        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            throw new InvalidOperationException("Application code is required.");
        }

        await using var dataSource = CreateDataSource();

        await using var command =
            dataSource.CreateCommand(
                """
                SELECT
                    id,
                    code,
                    name,
                    description,
                    base_url,
                    is_active,
                    created_at,
                    updated_at
                FROM public.applications
                WHERE LOWER(code) = LOWER(@code);
                """);

        command.Parameters.AddWithValue("code", normalizedCode);

        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? Map(reader) : null;
    }

    public async Task<ApplicationResponse> CreateAsync(CreateApplicationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var code = NormalizeCode(request.Code).ToUpperInvariant();
        var name = NormalizeName(request.Name);
        var description = NormalizeDescription(request.Description);
        var baseUrl = NormalizeBaseUrl(request.BaseUrl);

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new InvalidOperationException("Application code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Application name is required.");
        }

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Application base URL is required.");
        }

        await using var dataSource = CreateDataSource();

        await using var duplicateCommand =
            dataSource.CreateCommand(
                """
                SELECT 1
                FROM public.applications
                WHERE LOWER(code) = LOWER(@code);
                """);

        duplicateCommand.Parameters.AddWithValue("code", code);

        await using var duplicateReader = await duplicateCommand.ExecuteReaderAsync();

        if (await duplicateReader.ReadAsync())
        {
            throw new InvalidOperationException($"Application code '{code}' already exists.");
        }

        var now = DateTime.UtcNow;
        var id = Guid.NewGuid();

        await using var insertCommand =
            dataSource.CreateCommand(
                """
                INSERT INTO public.applications
                (
                    id,
                    code,
                    name,
                    description,
                    base_url,
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
                    @base_url,
                    @is_active,
                    @created_at,
                    @updated_at
                );
                """);

        insertCommand.Parameters.AddWithValue("id", id);
        insertCommand.Parameters.AddWithValue("code", code);
        insertCommand.Parameters.AddWithValue("name", name);
        insertCommand.Parameters.AddWithValue("description", description);
        insertCommand.Parameters.AddWithValue("base_url", baseUrl);
        insertCommand.Parameters.AddWithValue("is_active", true);
        insertCommand.Parameters.AddWithValue("created_at", now);
        insertCommand.Parameters.AddWithValue("updated_at", now);

        await insertCommand.ExecuteNonQueryAsync();

        await using var selectCommand =
            dataSource.CreateCommand(
                """
                SELECT
                    id,
                    code,
                    name,
                    description,
                    base_url,
                    is_active,
                    created_at,
                    updated_at
                FROM public.applications
                WHERE id = @id;
                """);

        selectCommand.Parameters.AddWithValue("id", id);

        await using var createdReader = await selectCommand.ExecuteReaderAsync();

        if (!await createdReader.ReadAsync())
        {
            throw new InvalidOperationException("Application was not found after create.");
        }

        return Map(createdReader);
    }

    public async Task<ApplicationResponse> UpdateAsync(
        string currentCode,
        UpdateApplicationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedCurrentCode = NormalizeCode(currentCode);
        var name = NormalizeName(request.Name);
        var description = NormalizeDescription(request.Description);
        var baseUrl = NormalizeBaseUrl(request.BaseUrl);

        if (string.IsNullOrWhiteSpace(normalizedCurrentCode))
        {
            throw new InvalidOperationException("Current application code is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Application name is required.");
        }

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            throw new InvalidOperationException("Application base URL is required.");
        }

        await using var dataSource = CreateDataSource();

        await using var findCommand =
            dataSource.CreateCommand(
                """
                SELECT
                    id,
                    code,
                    name,
                    description,
                    base_url,
                    is_active,
                    created_at,
                    updated_at
                FROM public.applications
                WHERE LOWER(code) = LOWER(@code);
                """);

        findCommand.Parameters.AddWithValue("code", normalizedCurrentCode);

        await using var currentReader = await findCommand.ExecuteReaderAsync();

        if (!await currentReader.ReadAsync())
        {
            throw new InvalidOperationException($"Application '{normalizedCurrentCode}' not found.");
        }

        var current = Map(currentReader);
        var now = DateTime.UtcNow;

        await using var updateCommand =
            dataSource.CreateCommand(
                """
                UPDATE public.applications
                SET
                    name = @name,
                    description = @description,
                    base_url = @base_url,
                    updated_at = @updated_at
                WHERE id = @id;
                """);

        updateCommand.Parameters.AddWithValue("name", name);
        updateCommand.Parameters.AddWithValue("description", description);
        updateCommand.Parameters.AddWithValue("base_url", baseUrl);
        updateCommand.Parameters.AddWithValue("updated_at", now);
        updateCommand.Parameters.AddWithValue("id", current.Id);

        var affected = await updateCommand.ExecuteNonQueryAsync();

        if (affected == 0)
        {
            throw new InvalidOperationException($"Application '{normalizedCurrentCode}' was not updated.");
        }

        await using var refreshedCommand =
            dataSource.CreateCommand(
                """
                SELECT
                    id,
                    code,
                    name,
                    description,
                    base_url,
                    is_active,
                    created_at,
                    updated_at
                FROM public.applications
                WHERE id = @id;
                """);

        refreshedCommand.Parameters.AddWithValue("id", current.Id);

        await using var refreshedReader = await refreshedCommand.ExecuteReaderAsync();

        if (!await refreshedReader.ReadAsync())
        {
            throw new InvalidOperationException($"Application '{current.Code}' was not found after update.");
        }

        return Map(refreshedReader);
    }

    public async Task DeactivateAsync(string code)
    {
        var normalizedCode = NormalizeCode(code);

        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            throw new InvalidOperationException("Application code is required.");
        }

        await using var dataSource = CreateDataSource();

        await using var findCommand =
            dataSource.CreateCommand(
                """
                SELECT id
                FROM public.applications
                WHERE LOWER(code) = LOWER(@code);
                """);

        findCommand.Parameters.AddWithValue("code", normalizedCode);

        await using var currentReader = await findCommand.ExecuteReaderAsync();

        if (!await currentReader.ReadAsync())
        {
            throw new InvalidOperationException($"Application '{normalizedCode}' not found.");
        }

        var id = currentReader.GetGuid(0);
        var now = DateTime.UtcNow;

        await using var updateCommand =
            dataSource.CreateCommand(
                """
                UPDATE public.applications
                SET
                    is_active = @is_active,
                    updated_at = @updated_at
                WHERE id = @id;
                """);

        updateCommand.Parameters.AddWithValue("is_active", false);
        updateCommand.Parameters.AddWithValue("updated_at", now);
        updateCommand.Parameters.AddWithValue("id", id);

        await updateCommand.ExecuteNonQueryAsync();
    }

    private static ApplicationResponse Map(NpgsqlDataReader reader)
    {
        return new ApplicationResponse
        {
            Id = reader.GetGuid(0),
            Code = reader.GetString(1),
            Name = reader.GetString(2),
            Description = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
            BaseUrl = reader.GetString(4),
            IsActive = reader.GetBoolean(5),
            CreatedAt = reader.GetDateTime(6),
            UpdatedAt = reader.GetDateTime(7)
        };
    }

    private static string NormalizeCode(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static string NormalizeName(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static string NormalizeDescription(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }

    private static string NormalizeBaseUrl(string? value)
    {
        return value?.Trim() ?? string.Empty;
    }
}
