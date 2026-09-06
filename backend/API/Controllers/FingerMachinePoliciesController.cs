using BDIP.Application.FingerMachinePolicies;
using BDIP.Contracts.FingerMachinePolicies;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/finger-machines/{machineCode}/policy")]
public sealed class FingerMachinePoliciesController
    : ControllerBase
{
    private readonly IFingerMachinePolicyService _service;

    public FingerMachinePoliciesController(
        IFingerMachinePolicyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(
        string machineCode)
    {
        var policy =
            await _service.GetByMachineCodeAsync(
                machineCode);

        if (policy is null)
        {
            return NotFound(new
            {
                success = false,
                message =
                    $"Machine '{machineCode}' not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = policy
        });
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAsync(
        string machineCode,
        [FromBody] UpdateFingerMachinePolicyRequest request)
    {
        try
        {
            var policy =
                await _service.UpsertAsync(
                    machineCode,
                    request);

            return Ok(new
            {
                success = true,
                message =
                    "Finger machine policy updated successfully.",
                data = policy
            });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                success = false,
                message = exception.Message
            });
        }
    }
}
