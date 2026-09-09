using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers.Attendance;

[ApiController]
[Route("api/attendance/hris-status")]
public sealed class HrisStatusSyncController : ControllerBase
{
    private readonly IHrisStatusSyncService _service;

    public HrisStatusSyncController(
        IHrisStatusSyncService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> SyncStatus(
        [FromBody] AttendanceHrisStatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _service.SyncStatusAsync(
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
