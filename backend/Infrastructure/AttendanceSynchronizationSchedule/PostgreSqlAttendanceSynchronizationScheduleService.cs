using BDIP.Application.AttendanceSynchronizationSchedule;
using BDIP.Contracts.AttendanceSynchronizationSchedule;
using BDIP.Persistence.PostgreSQL;

using Microsoft.Extensions.Options;

using Npgsql;

namespace BDIP.Infrastructure.AttendanceSynchronizationSchedule;

public sealed class PostgreSqlAttendanceSynchronizationScheduleService
    : IAttendanceSynchronizationScheduleService,
      IAttendanceSynchronizationScheduleStateService
{
    private readonly ApplicationDbOptions _options;

    public PostgreSqlAttendanceSynchronizationScheduleService(
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
            SslMode = SslMode.Disable
        };

        return NpgsqlDataSource.Create(builder.ConnectionString);
    }

    public async Task UpdateExecutionStateAsync(
        Guid scheduleId,
        DateTimeOffset startedAt,
        DateTimeOffset finishedAt,
        bool success,
        string message,
        CancellationToken cancellationToken = default)
    {
        await using var dataSource = CreateDataSource();

        await using var connection =
            await dataSource.OpenConnectionAsync(cancellationToken);

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            """
            UPDATE public.attendance_synchronization_schedule
            SET
                last_started_at = @last_started_at,
                last_finished_at = @last_finished_at,
                last_success = @last_success,
                last_message = @last_message,
                updated_at = NOW()
            WHERE id = @id;
            """;

        command.Parameters.AddWithValue(
            "id",
            scheduleId);

        command.Parameters.AddWithValue(
            "last_started_at",
            startedAt);

        command.Parameters.AddWithValue(
            "last_finished_at",
            finishedAt);

        command.Parameters.AddWithValue(
            "last_success",
            success);

        command.Parameters.AddWithValue(
            "last_message",
            message);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static DateTimeOffset? CalculateNextSyncAt(
        bool isEnabled,
        string frequency,
        TimeSpan syncTime,
        short? weekday,
        short? dayOfMonth)
    {
        if (!isEnabled)
        {
            return null;
        }

        var now = DateTimeOffset.Now;
        var today = now.Date;

        if (frequency == "DAILY")
        {
            var candidate = new DateTimeOffset(
                DateOnly.FromDateTime(today),
                TimeOnly.FromTimeSpan(syncTime),
                now.Offset);

            return candidate > now
                ? candidate
                : candidate.AddDays(1);
        }

        if (frequency == "WEEKLY")
        {
            if (weekday is null)
            {
                return null;
            }

            var targetDay = weekday.Value switch
            {
                1 => DayOfWeek.Monday,
                2 => DayOfWeek.Tuesday,
                3 => DayOfWeek.Wednesday,
                4 => DayOfWeek.Thursday,
                5 => DayOfWeek.Friday,
                6 => DayOfWeek.Saturday,
                7 => DayOfWeek.Sunday,
                _ => throw new ArgumentOutOfRangeException(nameof(weekday)),
            };

            var daysUntil = ((int)targetDay - (int)today.DayOfWeek + 7) % 7;

            var candidate = new DateTimeOffset(
                DateOnly.FromDateTime(today.AddDays(daysUntil)),
                TimeOnly.FromTimeSpan(syncTime),
                now.Offset);

            return candidate > now
                ? candidate
                : candidate.AddDays(7);
        }

        if (frequency == "MONTHLY")
        {
            if (dayOfMonth is null)
            {
                return null;
            }

            var year = today.Year;
            var month = today.Month;
            var daysInMonth = DateTime.DaysInMonth(year, month);

            if (dayOfMonth.Value <= daysInMonth)
            {
                var candidate = new DateTimeOffset(
                    DateOnly.FromDateTime(
                        new DateTime(year, month, dayOfMonth.Value)),
                    TimeOnly.FromTimeSpan(syncTime),
                    now.Offset);

                if (candidate > now)
                {
                    return candidate;
                }
            }

            var nextMonth = today.AddMonths(1);
            var nextDaysInMonth =
                DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);

            if (dayOfMonth.Value > nextDaysInMonth)
            {
                return null;
            }

            return new DateTimeOffset(
                DateOnly.FromDateTime(
                    new DateTime(
                        nextMonth.Year,
                        nextMonth.Month,
                        dayOfMonth.Value)),
                TimeOnly.FromTimeSpan(syncTime),
                now.Offset);
        }

        return null;
    }

    public async Task<AttendanceSynchronizationScheduleResponse> GetAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dataSource = CreateDataSource();

        await using var command = dataSource.CreateCommand(
            """
            SELECT
                id,
                is_enabled,
                frequency,
                sync_time,
                weekday,
                day_of_month,
                last_started_at,
                last_finished_at,
                last_success,
                last_message
            FROM public.attendance_synchronization_schedule
            LIMIT 1;
            """
        );

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "Konfigurasi scheduler Attendance belum tersedia.");
        }

        return new AttendanceSynchronizationScheduleResponse
        {
            Id = reader.GetGuid(0),
            IsEnabled = reader.GetBoolean(1),
            Frequency = reader.GetString(2),
            SyncTime = reader.GetTimeSpan(3).ToString(@"hh\:mm"),
            Weekday = reader.IsDBNull(4) ? null : reader.GetInt16(4),
            DayOfMonth = reader.IsDBNull(5) ? null : reader.GetInt16(5),
            LastStartedAt = reader.IsDBNull(6)
                ? null
                : reader.GetFieldValue<DateTimeOffset>(6),
            LastFinishedAt = reader.IsDBNull(7)
                ? null
                : reader.GetFieldValue<DateTimeOffset>(7),
            LastSuccess = reader.IsDBNull(8)
                ? null
                : reader.GetBoolean(8),
            LastMessage = reader.IsDBNull(9)
                ? null
                : reader.GetString(9),
            NextSyncAt = CalculateNextSyncAt(
                reader.GetBoolean(1),
                reader.GetString(2),
                reader.GetTimeSpan(3),
                reader.IsDBNull(4) ? null : reader.GetInt16(4),
                reader.IsDBNull(5) ? null : reader.GetInt16(5))
        };
    }

    public async Task<AttendanceSynchronizationScheduleResponse> UpdateAsync(
        UpdateAttendanceSynchronizationScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var frequency = request.Frequency.Trim().ToUpperInvariant();

        if (frequency is not ("DAILY" or "WEEKLY" or "MONTHLY"))
        {
            throw new ArgumentException(
                "Frequency harus DAILY, WEEKLY, atau MONTHLY.");
        }

        if (!TimeSpan.TryParse(
                request.SyncTime,
                out var syncTime))
        {
            throw new ArgumentException(
                "SyncTime harus menggunakan format HH:mm.");
        }

        if (frequency == "WEEKLY" &&
            (!request.Weekday.HasValue ||
             request.Weekday.Value < 1 ||
             request.Weekday.Value > 7))
        {
            throw new ArgumentException(
                "Weekly membutuhkan weekday 1 sampai 7.");
        }

        if (frequency == "MONTHLY" &&
            (!request.DayOfMonth.HasValue ||
             request.DayOfMonth.Value < 1 ||
             request.DayOfMonth.Value > 31))
        {
            throw new ArgumentException(
                "Monthly membutuhkan dayOfMonth 1 sampai 31.");
        }

        await using var dataSource = CreateDataSource();

        await using var connection =
            await dataSource.OpenConnectionAsync(cancellationToken);

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            """
            UPDATE public.attendance_synchronization_schedule
            SET
                is_enabled = @is_enabled,
                frequency = @frequency,
                sync_time = @sync_time,
                weekday = @weekday,
                day_of_month = @day_of_month,
                updated_at = NOW()
            WHERE id = (
                SELECT id
                FROM public.attendance_synchronization_schedule
                LIMIT 1
            );
            """;

        command.Parameters.AddWithValue(
            "is_enabled",
            request.IsEnabled);

        command.Parameters.AddWithValue(
            "frequency",
            frequency);

        command.Parameters.AddWithValue(
            "sync_time",
            syncTime);

        command.Parameters.AddWithValue(
            "weekday",
            request.Weekday.HasValue
                ? request.Weekday.Value
                : DBNull.Value);

        command.Parameters.AddWithValue(
            "day_of_month",
            request.DayOfMonth.HasValue
                ? request.DayOfMonth.Value
                : DBNull.Value);

        await command.ExecuteNonQueryAsync(cancellationToken);

        return await GetAsync(cancellationToken);
    }
}
