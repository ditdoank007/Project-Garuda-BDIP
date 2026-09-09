using BDIP.Contracts.Attendance;

namespace BDIP.Application.Attendance;

public interface IAttendanceDiscoveryService
{
    Task<AttendanceDiscoveryResponse> DiscoverAsync(
        string machineCode,
        CancellationToken cancellationToken = default);

    Task<AttendanceDiscoveryDeleteResponse> DeleteAsync(
        AttendanceDiscoveryDeleteRequest request,
        CancellationToken cancellationToken = default);
}
