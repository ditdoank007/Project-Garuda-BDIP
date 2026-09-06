using BDIP.Contracts.FingerMachineRuntime;

namespace BDIP.Application.FingerMachineRuntime;

public interface IFingerMachineRuntimeService
{
    Task<IEnumerable<FingerMachineRuntimeResponse>>
        GetAllAsync();

    Task RecordRuntimeAsync(
        string machineCode,
        FingerMachineRuntimeRequest request);

    Task RecordPullResultAsync(
        string machineCode,
        FingerMachinePullResultRequest request);

    Task RecordSyncResultAsync(
        string machineCode,
        FingerMachineSyncResultRequest request);

    Task RecordClearResultAsync(
        string machineCode,
        FingerMachineClearResultRequest request);

    Task<object> ManualPullAsync(
        string machineCode);
}
