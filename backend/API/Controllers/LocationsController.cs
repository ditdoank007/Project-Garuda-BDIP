using BDIP.Application.Locations;
using BDIP.Contracts.Locations;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/locations")]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _locationService;

    public LocationsController(
        ILocationService locationService)
    {
        _locationService = locationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var locations =
            await _locationService.GetAllAsync();

        return Ok(new
        {
            success = true,
            data = locations
        });
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetByName(
        string name)
    {
        var location =
            await _locationService.GetByNameAsync(name);

        if (location is null)
        {
            return NotFound(new
            {
                success = false,
                message =
                    $"Location '{name}' not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = location
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CreateLocationRequest request)
    {
        try
        {
            var location =
                await _locationService.CreateAsync(
                    request);

            return Ok(new
            {
                success = true,
                message =
                    "Location created successfully.",
                data = location
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

    [HttpPut("{name}")]
    public async Task<IActionResult> Update(
        string name,
        UpdateLocationRequest request)
    {
        try
        {
            var location =
                await _locationService.UpdateAsync(
                    name,
                    request);

            return Ok(new
            {
                success = true,
                message =
                    "Location updated successfully.",
                data = location
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

    [HttpDelete("{name}")]
    public async Task<IActionResult> Delete(
        string name)
    {
        try
        {
            await _locationService.DeleteAsync(name);

            return Ok(new
            {
                success = true,
                message =
                    "Location deleted successfully."
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
