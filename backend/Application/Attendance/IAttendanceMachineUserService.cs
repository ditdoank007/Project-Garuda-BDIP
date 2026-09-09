using BDIP.Contracts.Attendance;

namespace BDIP.Application.Attendance;

public interface IAttendanceMachineUserService
{
    Task<AttendanceMachineUserEnabledResponse> SetEnabledAsync(
        AttendanceMachineUserEnabledRequest request,
        CancellationToken cancellationToken = default);
}
