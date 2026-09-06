using BDIP.Application.FingerMachineGlobalPolicy;
using BDIP.Contracts.FingerMachineGlobalPolicy;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/finger-machines/global-policy")]
public sealed class FingerMachineGlobalPolicyController
    : ControllerBase
{
    private readonly IFingerMachineGlobalPolicyService _service;

    public FingerMachineGlobalPolicyController(
        IFingerMachineGlobalPolicyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        try
        {
            var policy =
                await _service.GetAsync();

            return Ok(new
            {
                success = true,
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

    [HttpPut]
    public async Task<IActionResult> UpdateAsync(
        [FromBody]
        UpdateFingerMachineGlobalPolicyRequest request)
    {
        try
        {
            var policy =
                await _service.UpdateAsync(request);

            return Ok(new
            {
                success = true,
                message =
                    "Finger machine global policy updated successfully.",
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
