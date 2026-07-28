using BDIP.Application.Roles;
using BDIP.Contracts.Roles;

using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/roles")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(
        IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var roles =
            await _roleService.GetAllAsync();

        return Ok(new
        {
            success = true,
            data = roles
        });
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetByName(
        string name)
    {
        var role =
            await _roleService.GetByNameAsync(name);

        if (role is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Role not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = role
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateRoleRequest request)
    {
        try
        {
            var role =
                await _roleService.CreateAsync(request);

            return Ok(new
            {
                success = true,
                message = "Role created successfully.",
                data = role
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
        [FromBody] UpdateRoleRequest request)
    {
        try
        {
            var role =
                await _roleService.UpdateAsync(
                    name,
                    request);

            if (role is null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Role not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Role updated successfully.",
                data = role
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

    [HttpGet("{name}/members")]
    public async Task<IActionResult> GetMembers(
        string name)
    {
        var result =
            await _roleService.GetMembersAsync(name);

        if (result is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Role not found."
            });
        }

        return Ok(new
        {
            success = true,
            data = result
        });
    }

    [HttpPost("{name}/members")]
    public async Task<IActionResult> AddMember(
        string name,
        [FromBody] UpdateRoleMemberRequest request)
    {
        try
        {
            await _roleService.AddMemberAsync(
                name,
                request.Username);

            return Ok(new
            {
                success = true,
                message = "Member added successfully."
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

    [HttpDelete("{name}/members/{username}")]
    public async Task<IActionResult> RemoveMember(
        string name,
        string username)
    {
        try
        {
            await _roleService.RemoveMemberAsync(
                name,
                username);

            return Ok(new
            {
                success = true,
                message = "Member removed successfully."
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
                await _roleService.DeleteAsync(name);

            if (!deleted)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Role not found."
                });
            }

            return Ok(new
            {
                success = true,
                message = "Role deleted successfully."
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
