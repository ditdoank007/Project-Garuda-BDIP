using BDIP.Application.NAP;
using BDIP.Application.Users;
using BDIP.Contracts.NAP;

namespace BDIP.Infrastructure.NAP;

public sealed class NapSynchronizationService : INapSynchronizationService
{
    private readonly IUserService _userService;
    private readonly IUserNapService _userNapService;
    private readonly IPolicyService _policyService;
    private readonly INapLdapGroupSyncService _ldapGroupSyncService;

    public NapSynchronizationService(
        IUserService userService,
        IUserNapService userNapService,
        IPolicyService policyService,
        INapLdapGroupSyncService ldapGroupSyncService)
    {
        _userService = userService;
        _userNapService = userNapService;
        _policyService = policyService;
        _ldapGroupSyncService = ldapGroupSyncService;
    }

    public async Task<NapSynchronizationResult> SynchronizeAsync()
    {
        var result = new NapSynchronizationResult();

        var users = await _userService.GetUsersAsync();

        var policy =
            await _policyService.GetByCodeAsync("DEFAULT")
            ?? throw new InvalidOperationException(
                "Policy DEFAULT not found.");

        var existing =
            await _userNapService.GetAllAsync();

        var existingLookup =
            existing.ToDictionary(
                x => x.Uid,
                StringComparer.OrdinalIgnoreCase);

        result.LdapUsers = users.Users.Count;

        foreach (var user in users.Users)
        {
            try
            {
                if (existingLookup.ContainsKey(user.Username))
                {
                    result.ExistingUsers++;
                    continue;
                }

                await _userNapService.UpdatePolicyAsync(
                    user.Username,
                    policy.Id,
                    policy.Code);

                result.InsertedUsers++;
            }
            catch
            {
                result.FailedUsers++;
            }
        }

        // BDIP NAP is the source of truth.
        // Reconcile LDAP groups after user NAP synchronization.
        await _ldapGroupSyncService.SyncAllAsync();

        return result;
    }
}
