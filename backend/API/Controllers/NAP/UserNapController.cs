using BDIP.Application.NAP;
using BDIP.Domain.NAP;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

public sealed class UpdateUserPolicyRequest
{
    public Guid? PolicyId { get; set; }

    public string? PolicyCode { get; set; }
}

[ApiController]
[Route("api/nap/users")]
public class UserNapController : ControllerBase
{
    private readonly IUserNapService _userNapService;
    private readonly IPolicyService _policyService;

    public UserNapController(
        IUserNapService userNapService,
        IPolicyService policyService)
    {
        _userNapService = userNapService;
        _policyService = policyService;
    }

    [HttpGet("{uid}")]
    public async Task<IActionResult> GetByUid(string uid)
    {
        var userNap =
            await _userNapService.GetByUidAsync(uid);

        if (userNap is null)
        {
            return NotFound(new
            {
                success = false,
                message = $"User '{uid}' not found."
            });
        }

        Policy? policy = null;

        if (!string.IsNullOrWhiteSpace(userNap.PolicyCode))
        {
            policy =
                await _policyService.GetByCodeAsync(
                    userNap.PolicyCode);
        }

        return Ok(new
        {
            success = true,
            data = new
            {
                userNap.Uid,
                userNap.DownloadKbps,
                userNap.UploadKbps,
                userNap.SessionTimeout,
                userNap.IdleTimeout,
                userNap.PolicyCode,
                Policy = policy,
                userNap.IsActive,
                userNap.CreatedAt,
                userNap.UpdatedAt
            }
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users =
            await _userNapService.GetAllAsync();

        return Ok(new
        {
            success = true,
            data = users
        });
    }

    [HttpPut("{uid}/policy")]
    public async Task<IActionResult> UpdatePolicy(
        string uid,
        [FromBody] UpdateUserPolicyRequest request)
    {
        Policy? policy = null;

        if (request.PolicyId.HasValue)
        {
            policy = await _policyService.GetByIdAsync(
                request.PolicyId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(request.PolicyCode))
        {
            policy = await _policyService.GetByCodeAsync(
                request.PolicyCode);
        }
        else
        {
            return BadRequest(new
            {
                success = false,
                message = "PolicyId or PolicyCode is required."
            });
        }

        if (policy is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Policy not found."
            });
        }

        var updated =
            await _userNapService.UpdatePolicyAsync(
                uid,
                policy.Id,
                policy.Code);

        return Ok(new
        {
            success = true,
            message = "User policy updated successfully.",
            data = updated
        });
    }
}
