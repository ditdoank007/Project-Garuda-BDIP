using BDIP.Application.AttendanceSynchronizationSchedule;
using BDIP.Contracts.AttendanceSynchronizationSchedule;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("/api/attendance/synchronization/schedule")]
public sealed class AttendanceSynchronizationScheduleController
    : ControllerBase
{
    private readonly IAttendanceSynchronizationScheduleService _service;

    public AttendanceSynchronizationScheduleController(
        IAttendanceSynchronizationScheduleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _service.GetAsync(cancellationToken);

            return Ok(new
            {
                success = true,
                data = result
            });
        }
        catch (Exception exception)
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
        [FromBody] UpdateAttendanceSynchronizationScheduleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _service.UpdateAsync(
                    request,
                    cancellationToken);

            return Ok(new
            {
                success = true,
                data = result
            });
        }
        catch (Exception exception)
        {
            return BadRequest(new
            {
                success = false,
                message = exception.Message
            });
        }
    }
}
