using BDIP.Application.Attendance;
using BDIP.Contracts.Attendance;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers.Attendance;

[ApiController]
[Route("api/attendance/master")]
public sealed class AttendanceMasterController : ControllerBase
{
    private readonly IAttendanceMasterService _service;

    public AttendanceMasterController(
        IAttendanceMasterService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<AttendanceMasterResponse>> Get(
        CancellationToken cancellationToken)
    {
        var result = await _service.GetAsync(cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Save(
        [FromBody] AttendanceMasterSaveRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.SaveAsync(request, cancellationToken);

            return Ok(new
            {
                success = true,
                message = "Master Attendance berhasil disimpan."
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
    }
}
