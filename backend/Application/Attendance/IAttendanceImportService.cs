using BDIP.Contracts.Attendance;

namespace BDIP.Application.Attendance;

public interface IAttendanceImportService
{
    Task<AttendanceImportResponse> ImportAsync(
        string machineCode,
        CancellationToken cancellationToken = default);
}
