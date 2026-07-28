using BDIP.Application.NAP;
using BDIP.Domain.NAP;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/policies")]
public class PoliciesController : ControllerBase
{
    private readonly IPolicyService _policyService;

    public PoliciesController(
        IPolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var policies =
            await _policyService.GetAllAsync();

        return Ok(new
        {
            success = true,
            data = policies
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id)
    {
        var policy =
            await _policyService.GetByIdAsync(id);

        if (policy is null)
        {
            return NotFound(new
            {
                success = false,
                message = $"Policy '{id}' not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = policy
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] Policy policy)
    {
        try
        {
            var existing =
                await _policyService.GetByCodeAsync(policy.Code);

            if (existing is not null)
            {
                return Conflict(new
                {
                    success = false,
                    message = $"Policy code '{policy.Code}' already exists."
                });
            }


            var created =
                await _policyService.CreateAsync(policy);

            return Ok(new
            {
                success = true,
                message = "Policy created successfully.",
                data = created
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
        [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] Policy policy)
    {
        if (id != policy.Id)
        {
            return BadRequest(new
            {
                success = false,
                message = "Route id does not match policy id."
            });
        }

        var existing =
            await _policyService.GetByCodeAsync(policy.Code);

        if (existing is not null &&
            existing.Id != policy.Id)
        {
            return Conflict(new
            {
                success = false,
                message = $"Policy code '{policy.Code}' already exists."
            });
        }

        try
        {
            var updated =
                await _policyService.UpdateAsync(policy);

            return Ok(new
            {
                success = true,
                message = "Policy updated successfully.",
                data = updated
            });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new
            {
                success = false,
                message = $"Policy '{id}' not found."
            });
        }
    }
        [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id)
    {
        var policy =
            await _policyService.GetByIdAsync(id);

        if (policy is null)
        {
            return NotFound(new
            {
                success = false,
                message = $"Policy '{id}' not found."
            });
        }

        await _policyService.DeleteAsync(id);

        return Ok(new
        {
            success = true,
            message = "Policy deleted successfully."
        });
    }
}