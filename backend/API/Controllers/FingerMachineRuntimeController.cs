using BDIP.Application.FingerMachineRuntime;
using BDIP.Contracts.FingerMachineRuntime;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/finger-machines/{machineCode}/runtime")]
public sealed class FingerMachineRuntimeController
    : ControllerBase
{
    private readonly IFingerMachineRuntimeService _service;

    public FingerMachineRuntimeController(
        IFingerMachineRuntimeService service)
    {
        _service = service;
    }

    [HttpGet("/api/finger-machines/runtime/status")]
    public async Task<IActionResult> GetAllAsync()
    {
        try
        {
            var runtime =
                await _service.GetAllAsync();

            return Ok(new
            {
                success = true,
                data = runtime
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


    [HttpPost]
    public async Task<IActionResult> RecordRuntimeAsync(
        string machineCode,
        [FromBody] FingerMachineRuntimeRequest request)
    {
        try
        {
            await _service.RecordRuntimeAsync(
                machineCode,
                request);

            return Ok(new
            {
                success = true,
                message =
                    "Finger machine runtime recorded successfully."
            });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                success = false,
                message = exception.Message
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

    [HttpPost("pull")]
    public async Task<IActionResult> RecordPullResultAsync(
        string machineCode,
        [FromBody] FingerMachinePullResultRequest request)
    {
        try
        {
            await _service.RecordPullResultAsync(
                machineCode,
                request);

            return Ok(new
            {
                success = true,
                message =
                    "Finger machine pull result recorded successfully."
            });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                success = false,
                message = exception.Message
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

    [HttpPost("sync")]
    public async Task<IActionResult> RecordSyncResultAsync(
        string machineCode,
        [FromBody] FingerMachineSyncResultRequest request)
    {
        try
        {
            await _service.RecordSyncResultAsync(
                machineCode,
                request);

            return Ok(new
            {
                success = true,
                message =
                    "Finger machine sync result recorded successfully."
            });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                success = false,
                message = exception.Message
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

    [HttpPost("clear")]
    public async Task<IActionResult> RecordClearResultAsync(
        string machineCode,
        [FromBody] FingerMachineClearResultRequest request)
    {
        try
        {
            await _service.RecordClearResultAsync(
                machineCode,
                request);

            return Ok(new
            {
                success = true,
                message =
                    "Finger machine clear result recorded successfully."
            });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                success = false,
                message = exception.Message
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
    [HttpPost("manual-pull")]
    public async Task<IActionResult> ManualPullAsync(
        string machineCode)
    {
        try
        {
            var result =
                await _service.ManualPullAsync(
                    machineCode);

            return Ok(new
            {
                success = true,
                data = result
            });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new
            {
                success = false,
                message = exception.Message
            });
        }
        catch (Exception exception)
        {
            return BadRequest(new
            {
                success = false,
                message = exception.Message,
                inner = exception.InnerException?.Message
            });
        }
    }


}
