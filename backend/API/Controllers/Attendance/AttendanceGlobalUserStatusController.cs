using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers.Attendance;

[ApiController]
[Route("api/attendance/global-user-status")]
public sealed class AttendanceGlobalUserStatusController
    : ControllerBase
{
    private readonly IAttendanceGlobalUserStatusService _service;

    public AttendanceGlobalUserStatusController(
        IAttendanceGlobalUserStatusService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> SetStatus(
        [FromBody] AttendanceGlobalUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _service.SetStatusAsync(
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
