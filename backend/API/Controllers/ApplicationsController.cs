using BDIP.Application.Applications;
using BDIP.Contracts.Applications;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/applications")]
public sealed class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationsController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var applications = await _applicationService.GetAllAsync();

        return Ok(new
        {
            success = true,
            data = applications
        });
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCodeAsync(string code)
    {
        var application = await _applicationService.GetByCodeAsync(code);

        if (application is null)
        {
            return NotFound(new
            {
                success = false,
                message = $"Application '{code}' not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = application
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateApplicationRequest request)
    {
        try
        {
            var application = await _applicationService.CreateAsync(request);

            return Ok(new
            {
                success = true,
                message = "Application created successfully.",
                data = application
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
        [FromBody] UpdateApplicationRequest request)
    {
        try
        {
            var application = await _applicationService.UpdateAsync(code, request);

            return Ok(new
            {
                success = true,
                message = "Application updated successfully.",
                data = application
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
    public async Task<IActionResult> DeactivateAsync(string code)
    {
        try
        {
            await _applicationService.DeactivateAsync(code);

            return Ok(new
            {
                success = true,
                message = "Application deactivated successfully."
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
