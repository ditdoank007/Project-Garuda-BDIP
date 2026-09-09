using BDIP.Contracts.Attendance;

namespace BDIP.Application.Attendance;

public interface IAttendanceMasterMachineService
{
    Task<AttendanceMasterMachineResponse> GetAsync(
        CancellationToken cancellationToken = default);

    Task<AttendanceMasterMachineResponse> UpdateAsync(
        UpdateAttendanceMasterMachineRequest request,
        CancellationToken cancellationToken = default);
}
