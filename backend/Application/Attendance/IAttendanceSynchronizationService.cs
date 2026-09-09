using BDIP.Contracts.Attendance;

namespace BDIP.Application.Attendance;

public interface IAttendanceSynchronizationService
{
    Task<AttendanceSynchronizationResponse> SyncNowAsync(
        string? fingerId = null,
        CancellationToken cancellationToken = default);
}
