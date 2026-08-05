using BDIP.Contracts.NAP;

namespace BDIP.Application.NAP;

public interface INapSynchronizationService
{
    Task<NapSynchronizationResult> SynchronizeAsync();
}
