using BDIP.Application.FingerMachines;
using BDIP.Contracts.FingerMachines;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/finger-machines")]
public sealed class FingerMachinesController : ControllerBase
{
    private readonly IFingerMachineService _service;

    public FingerMachinesController(
        IFingerMachineService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var machines =
            await _service.GetAllAsync();

        return Ok(new
        {
            success = true,
            data = machines
        });
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCodeAsync(
        string code)
    {
        var machine =
            await _service.GetByCodeAsync(code);

        if (machine is null)
        {
            return NotFound(new
            {
                success = false,
                message =
                    $"Machine '{code}' not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = machine
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateFingerMachineRequest request)
    {
        try
        {
            var machine =
                await _service.CreateAsync(request);

            return Ok(new
            {
                success = true,
                message =
                    "Finger machine created successfully.",
                data = machine
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

    [HttpPut("{code}")]
    public async Task<IActionResult> UpdateAsync(
        string code,
        [FromBody] UpdateFingerMachineRequest request)
    {
        try
        {
            var machine =
                await _service.UpdateAsync(
                    code,
                    request);

            return Ok(new
            {
                success = true,
                message =
                    "Finger machine updated successfully.",
                data = machine
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

    [HttpDelete("{code}/deactivate")]
    public async Task<IActionResult> DeactivateAsync(
        string code)
    {
        try
        {
            await _service.DeactivateAsync(code);

            return Ok(new
            {
                success = true,
                message =
                    "Finger machine deactivated successfully."
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

    [HttpDelete("{code}")]
    public async Task<IActionResult> DeleteAsync(
        string code)
    {
        try
        {
            await _service.DeleteAsync(code);

            return Ok(new
            {
                success = true,
                message =
                    "Finger machine deleted successfully."
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
