namespace BDIP.Application.Attendance;

public interface IAttendanceReconciliationService
{
    Task<object> PreviewAsync(
        string machineCode,
        CancellationToken cancellationToken = default);
}
