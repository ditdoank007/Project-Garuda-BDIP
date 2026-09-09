using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/attendance/discovery")]
public sealed class AttendanceDiscoveryController
    : ControllerBase
{
    private readonly IAttendanceDiscoveryService _service;

    public AttendanceDiscoveryController(
        IAttendanceDiscoveryService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<AttendanceDiscoveryResponse>> Discover(
        [FromBody] AttendanceDiscoveryRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _service.DiscoverAsync(
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
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpPost("hapus-dari-mesin")]
    public async Task<ActionResult<AttendanceDiscoveryDeleteResponse>> HapusDariMesin(
        [FromBody] AttendanceDiscoveryDeleteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _service.DeleteAsync(
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
            return BadRequest(new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}
