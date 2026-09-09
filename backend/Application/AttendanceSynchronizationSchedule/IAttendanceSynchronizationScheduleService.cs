using BDIP.Contracts.AttendanceSynchronizationSchedule;

namespace BDIP.Application.AttendanceSynchronizationSchedule;

public interface IAttendanceSynchronizationScheduleService
{
    Task<AttendanceSynchronizationScheduleResponse> GetAsync(
        CancellationToken cancellationToken = default);

    Task<AttendanceSynchronizationScheduleResponse> UpdateAsync(
        UpdateAttendanceSynchronizationScheduleRequest request,
        CancellationToken cancellationToken = default);
}
