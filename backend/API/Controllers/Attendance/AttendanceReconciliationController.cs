using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers.Attendance;

[ApiController]
[Route("api/attendance/reconciliation")]
public sealed class AttendanceReconciliationController
    : ControllerBase
{
    private readonly IAttendanceReconciliationService _service;

    public AttendanceReconciliationController(
        IAttendanceReconciliationService service)
    {
        _service = service;
    }

    [HttpPost("preview")]
    public async Task<IActionResult> Preview(
        [FromBody] AttendanceReconciliationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.PreviewAsync(
                request.MachineCode,
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
            return StatusCode(502, new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}
