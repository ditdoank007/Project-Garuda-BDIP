using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers.Attendance;

[ApiController]
[Route("api/attendance/machine-user")]
public sealed class AttendanceMachineUserController : ControllerBase
{
    private readonly IAttendanceMachineUserService _service;

    public AttendanceMachineUserController(
        IAttendanceMachineUserService service)
    {
        _service = service;
    }

    [HttpPost("enabled")]
    public async Task<IActionResult> SetEnabled(
        [FromBody] AttendanceMachineUserEnabledRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _service.SetEnabledAsync(
                    request,
                    cancellationToken);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    success = false,
                    message = ex.Message
                });
        }
    }
}
