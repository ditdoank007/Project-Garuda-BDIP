using BDIP.Contracts.Attendance;

namespace BDIP.Application.Attendance;

public interface IAttendanceDiscoveryImportService
{
    Task<AttendanceDiscoveryImportResponse> ImportAsync(
        AttendanceDiscoveryImportRequest request,
        CancellationToken cancellationToken = default);
}
