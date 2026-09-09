using BDIP.Contracts.Attendance;

namespace BDIP.Application.Attendance;

public interface IHrisStatusSyncService
{
    Task<AttendanceHrisStatusResponse> SyncStatusAsync(
        AttendanceHrisStatusRequest request,
        CancellationToken cancellationToken = default);
}
