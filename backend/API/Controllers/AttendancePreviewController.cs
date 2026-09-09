using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/attendance")]
public sealed class AttendancePreviewController : ControllerBase
{
    private readonly IAttendancePreviewService _service;

    public AttendancePreviewController(
        IAttendancePreviewService service)
    {
        _service = service;
    }

    [HttpPost("preview")]
    public async Task<ActionResult<AttendancePreviewResponse>> Preview(
        [FromBody] AttendancePreviewRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.MachineCode))
        {
            return BadRequest(new
            {
                success = false,
                message = "MachineCode is required."
            });
        }

        try
        {
            var result =
                await _service.PreviewAsync(
                    request.MachineCode,
                    cancellationToken);

            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(502, new
            {
                success = false,
                message = $"Attendance collector unavailable: {ex.Message}"
            });
        }
    }
}
