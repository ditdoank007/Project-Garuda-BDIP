using BDIP.Contracts.Attendance;

namespace BDIP.Application.Attendance;

public interface IAttendancePreviewService
{
    Task<AttendancePreviewResponse> PreviewAsync(
        string machineCode,
        CancellationToken cancellationToken = default);
}
