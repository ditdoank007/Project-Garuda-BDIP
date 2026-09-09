using BDIP.Contracts.Attendance;

namespace BDIP.Application.Attendance;

public interface IAttendanceGlobalUserStatusService
{
    Task<AttendanceGlobalUserStatusResponse> SetStatusAsync(
        AttendanceGlobalUserStatusRequest request,
        CancellationToken cancellationToken = default);
}
