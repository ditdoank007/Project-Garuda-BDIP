using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers.Attendance;

[ApiController]
[Route("api/attendance/machine-snapshot")]
public sealed class AttendanceMachineSnapshotController
    : ControllerBase
{
    private readonly IAttendanceMachineSnapshotService _service;

    public AttendanceMachineSnapshotController(
        IAttendanceMachineSnapshotService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> GetSnapshot(
        [FromBody] AttendanceMachineSnapshotRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _service.GetSnapshotAsync(
                    request.MachineCode,
                    cancellationToken);

            return Content(
                (string)result,
                "application/json");
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
