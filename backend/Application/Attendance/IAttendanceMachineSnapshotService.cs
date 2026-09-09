namespace BDIP.Application.Attendance;

public interface IAttendanceMachineSnapshotService
{
    Task<object> GetSnapshotAsync(
        string machineCode,
        CancellationToken cancellationToken = default);
}
