using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers.Attendance;

[ApiController]
[Route("api/attendance/synchronization")]
public sealed class AttendanceSynchronizationController
    : ControllerBase
{
    private readonly IAttendanceSynchronizationService _service;

    public AttendanceSynchronizationController(
        IAttendanceSynchronizationService service)
    {
        _service = service;
    }

    [HttpPost("sync")]
    public async Task<ActionResult<AttendanceSynchronizationResponse>> Sync(
        [FromBody] AttendanceSynchronizationRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await _service.SyncNowAsync(
            request?.FingerId,
            cancellationToken);

        return Ok(result);
    }
}
