using BDIP.Contracts.Attendance;

namespace BDIP.Application.Attendance;

public interface IAttendanceMasterService
{
    Task<AttendanceMasterResponse> GetAsync(
        CancellationToken cancellationToken = default);

    Task SaveAsync(
        AttendanceMasterSaveRequest request,
        CancellationToken cancellationToken = default);
}
