using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/attendance/discovery")]
public sealed class AttendanceDiscoveryImportController : ControllerBase
{
    private readonly IAttendanceDiscoveryImportService _service;

    public AttendanceDiscoveryImportController(
        IAttendanceDiscoveryImportService service)
    {
        _service = service;
    }

    [HttpPost("import")]
    public async Task<ActionResult<AttendanceDiscoveryImportResponse>> Import(
        [FromBody] AttendanceDiscoveryImportRequest request,
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
            var result = await _service.ImportAsync(
                request,
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
