using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/attendance/master-machine")]
public sealed class AttendanceMasterMachineController
    : ControllerBase
{
    private readonly IAttendanceMasterMachineService _service;

    public AttendanceMasterMachineController(
        IAttendanceMasterMachineService service)
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
        [FromBody] UpdateAttendanceMasterMachineRequest request,
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
                message = "Master mesin finger berhasil diperbarui.",
                data = result
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
