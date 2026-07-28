using BDIP.Application.Units;
using BDIP.Contracts.Units;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/units")]
public class UnitsController : ControllerBase
{
    private readonly IUnitService _unitService;

    public UnitsController(IUnitService unitService)
    {
        _unitService = unitService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var units =
            await _unitService.GetAllAsync();

        return Ok(new
        {
            success = true,
            data = units
        });
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetByName(
        string name)
    {
        var unit =
            await _unitService.GetByNameAsync(name);

        if (unit is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Unit not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = unit
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUnitRequest request)
    {
        try
        {
            var unit =
                await _unitService.CreateAsync(request);

            return Ok(new
            {
                success = true,
                message = "Unit created successfully.",
                data = unit
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
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpPut("{name}")]
    public async Task<IActionResult> Update(
        string name,
        [FromBody] UpdateUnitRequest request)
    {
        try
        {
            var unit =
                await _unitService.UpdateAsync(
                    name,
                    request);

            if (unit is null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Unit not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Unit updated successfully.",
                data = unit
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
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    [HttpDelete("{name}")]
    public async Task<IActionResult> Delete(
        string name)
    {
        try
        {
            var deleted =
                await _unitService.DeleteAsync(name);

            if (!deleted)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Unit not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Unit deleted successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                success = false,
                message = ex.Message
            });
        }
    }
}
