using BDIP.Application.NAP;
using BDIP.Domain.NAP;
using BDIP.Persistence.PostgreSQL;
using BDIP.Application.Provisioning;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.NAP;

public sealed class PostgreSqlPolicyService : IPolicyService
{
    private readonly ApplicationDbOptions _options;
    private readonly IRadiusProvisioningService _radiusProvisioning;

    public PostgreSqlPolicyService(
        IOptions<ApplicationDbOptions> options,
        IRadiusProvisioningService radiusProvisioning)
    {
        _options = options.Value;
        _radiusProvisioning = radiusProvisioning;
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

                    session_timeout,
                    idle_timeout,
                    simultaneous_use,

                    download_rate,
                    upload_rate,
                    burst_download,
                    burst_upload,
                    priority,

                    daily_quota,
                    monthly_quota,
                    total_quota,

                    address_list,
                    vlan_id,
                    ip_pool,

                    enabled,
                    expiration_date,
                    login_schedule,

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

                    session_timeout,
                    idle_timeout,
                    simultaneous_use,

                    download_rate,
                    upload_rate,
                    burst_download,
                    burst_upload,
                    priority,

                    daily_quota,
                    monthly_quota,
                    total_quota,

                    address_list,
                    vlan_id,
                    ip_pool,

                    enabled,
                    expiration_date,
                    login_schedule,

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

                    session_timeout,
                    idle_timeout,
                    simultaneous_use,

                    download_rate,
                    upload_rate,
                    burst_download,
                    burst_upload,
                    priority,

                    daily_quota,
                    monthly_quota,
                    total_quota,

                    address_list,
                    vlan_id,
                    ip_pool,

                    enabled,
                    expiration_date,
                    login_schedule,

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

                        session_timeout,
                        idle_timeout,
                        simultaneous_use,

                        download_rate,
                        upload_rate,
                        burst_download,
                        burst_upload,
                        priority,

                        daily_quota,
                        monthly_quota,
                        total_quota,

                        address_list,
                        vlan_id,
                        ip_pool,

                        enabled,
                        expiration_date,
                        login_schedule,

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

                        @session_timeout,
                        @idle_timeout,
                        @simultaneous_use,

                        @download_rate,
                        @upload_rate,
                        @burst_download,
                        @burst_upload,
                        @priority,

                        @daily_quota,
                        @monthly_quota,
                        @total_quota,

                        @address_list,
                        @vlan_id,
                        @ip_pool,

                        @enabled,
                        @expiration_date,
                        @login_schedule,

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
        command.Parameters.AddWithValue("session_timeout", policy.SessionTimeout);
        command.Parameters.AddWithValue("idle_timeout", policy.IdleTimeout);
        command.Parameters.AddWithValue("simultaneous_use", policy.SimultaneousUse);

        command.Parameters.AddWithValue("download_rate", policy.DownloadRate);
        command.Parameters.AddWithValue("upload_rate", policy.UploadRate);

        command.Parameters.AddWithValue("burst_download",
            (object?)policy.BurstDownload ?? DBNull.Value);

        command.Parameters.AddWithValue("burst_upload",
            (object?)policy.BurstUpload ?? DBNull.Value);

        command.Parameters.AddWithValue("priority",
            (object?)policy.Priority ?? DBNull.Value);

        command.Parameters.AddWithValue("daily_quota",
            (object?)policy.DailyQuota ?? DBNull.Value);

        command.Parameters.AddWithValue("monthly_quota",
            (object?)policy.MonthlyQuota ?? DBNull.Value);

        command.Parameters.AddWithValue("total_quota",
            (object?)policy.TotalQuota ?? DBNull.Value);

        command.Parameters.AddWithValue("address_list",
            (object?)policy.AddressList ?? DBNull.Value);

        command.Parameters.AddWithValue("vlan_id",
            (object?)policy.VlanId ?? DBNull.Value);

        command.Parameters.AddWithValue("ip_pool",
            (object?)policy.IpPool ?? DBNull.Value);

        command.Parameters.AddWithValue("enabled", policy.Enabled);

        command.Parameters.AddWithValue("expiration_date",
            (object?)policy.ExpirationDate ?? DBNull.Value);

        command.Parameters.AddWithValue("login_schedule",
            (object?)policy.LoginSchedule ?? DBNull.Value);

        command.Parameters.AddWithValue("is_active", policy.IsActive);
        command.Parameters.AddWithValue("created_at", policy.CreatedAt);
        command.Parameters.AddWithValue("updated_at", policy.UpdatedAt);

        await command.ExecuteNonQueryAsync();
        await _radiusProvisioning.SyncPolicyAsync(policy);

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
                    code = @code,
                    name = @name,
                    description = @description,

                    session_timeout = @session_timeout,
                    idle_timeout = @idle_timeout,
                    simultaneous_use = @simultaneous_use,

                    download_rate = @download_rate,
                    upload_rate = @upload_rate,
                    burst_download = @burst_download,
                    burst_upload = @burst_upload,
                    priority = @priority,

                    daily_quota = @daily_quota,
                    monthly_quota = @monthly_quota,
                    total_quota = @total_quota,

                    address_list = @address_list,
                    vlan_id = @vlan_id,
                    ip_pool = @ip_pool,

                    enabled = @enabled,
                    expiration_date = @expiration_date,
                    login_schedule = @login_schedule,

                    is_active = @is_active,
                    updated_at = @updated_at
                WHERE id = @id;
                """);

        command.Parameters.AddWithValue("id", policy.Id);
        command.Parameters.AddWithValue("code", policy.Code);
        command.Parameters.AddWithValue("name", policy.Name);
        command.Parameters.AddWithValue("description",
            (object?)policy.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("session_timeout", policy.SessionTimeout);
        command.Parameters.AddWithValue("idle_timeout", policy.IdleTimeout);
        command.Parameters.AddWithValue("simultaneous_use", policy.SimultaneousUse);

        command.Parameters.AddWithValue("download_rate", policy.DownloadRate);
        command.Parameters.AddWithValue("upload_rate", policy.UploadRate);

        command.Parameters.AddWithValue("burst_download",
            (object?)policy.BurstDownload ?? DBNull.Value);

        command.Parameters.AddWithValue("burst_upload",
            (object?)policy.BurstUpload ?? DBNull.Value);

        command.Parameters.AddWithValue("priority",
            (object?)policy.Priority ?? DBNull.Value);

        command.Parameters.AddWithValue("daily_quota",
            (object?)policy.DailyQuota ?? DBNull.Value);

        command.Parameters.AddWithValue("monthly_quota",
            (object?)policy.MonthlyQuota ?? DBNull.Value);

        command.Parameters.AddWithValue("total_quota",
            (object?)policy.TotalQuota ?? DBNull.Value);

        command.Parameters.AddWithValue("address_list",
            (object?)policy.AddressList ?? DBNull.Value);

        command.Parameters.AddWithValue("vlan_id",
            (object?)policy.VlanId ?? DBNull.Value);

        command.Parameters.AddWithValue("ip_pool",
            (object?)policy.IpPool ?? DBNull.Value);

        command.Parameters.AddWithValue("enabled", policy.Enabled);

        command.Parameters.AddWithValue("expiration_date",
            (object?)policy.ExpirationDate ?? DBNull.Value);

        command.Parameters.AddWithValue("login_schedule",
            (object?)policy.LoginSchedule ?? DBNull.Value);    
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

        await _radiusProvisioning.SyncPolicyAsync(policy);
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
            Description = reader.IsDBNull(3)
                ? null
                : reader.GetString(3),

            // Session
            SessionTimeout = reader.GetInt32(4),
            IdleTimeout = reader.GetInt32(5),
            SimultaneousUse = reader.GetInt32(6),

            // Bandwidth
            DownloadRate = reader.GetInt32(7),
            UploadRate = reader.GetInt32(8),
            BurstDownload = reader.IsDBNull(9)
                ? null
                : reader.GetInt32(9),
            BurstUpload = reader.IsDBNull(10)
                ? null
                : reader.GetInt32(10),
            Priority = reader.IsDBNull(11)
                ? null
                : reader.GetInt32(11),

            // Quota
            DailyQuota = reader.IsDBNull(12)
                ? null
                : reader.GetInt64(12),
            MonthlyQuota = reader.IsDBNull(13)
                ? null
                : reader.GetInt64(13),
            TotalQuota = reader.IsDBNull(14)
                ? null
                : reader.GetInt64(14),

            // Network
            AddressList = reader.IsDBNull(15)
                ? null
                : reader.GetString(15),
            VlanId = reader.IsDBNull(16)
                ? null
                : reader.GetInt32(16),
            IpPool = reader.IsDBNull(17)
                ? null
                : reader.GetString(17),

            // Access
            Enabled = reader.GetBoolean(18),
            ExpirationDate = reader.IsDBNull(19)
                ? null
                : reader.GetFieldValue<DateTime>(19),
            LoginSchedule = reader.IsDBNull(20)
                ? null
                : reader.GetString(20),

            IsActive = reader.GetBoolean(21),
            CreatedAt = reader.GetFieldValue<DateTime>(22),
            UpdatedAt = reader.GetFieldValue<DateTime>(23)
        };
    }
}