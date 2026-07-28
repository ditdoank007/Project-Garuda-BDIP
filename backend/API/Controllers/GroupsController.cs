using BDIP.Application.Groups;
using BDIP.Contracts.Groups;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GroupResponse>>> GetAll()
    {
        var result = await _groupService.GetAllAsync();

        return Ok(result);
    }

    [HttpGet("{groupName}")]
    public async Task<ActionResult<GroupResponse>> GetByName(
        string groupName)
    {
        var result = await _groupService.GetByNameAsync(groupName);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("{groupName}/members")]
    public async Task<ActionResult<GroupMembersResponse>> GetMembers(
        string groupName)
    {
        var result = await _groupService.GetMembersAsync(groupName);

        if (result == null)
            return NotFound();

        return Ok(result);
    }



    [HttpPost]
    public async Task<ActionResult<GroupResponse>> Create(
        [FromBody] CreateGroupRequest request)
    {
        var result = await _groupService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetByName),
            new { groupName = result.Name },
            result);
    }

    [HttpPut("{groupName}")]
    public async Task<ActionResult<GroupResponse>> Update(
        string groupName,
        [FromBody] CreateGroupRequest request)
    {
        var result =
            await _groupService.UpdateAsync(
                groupName,
                request);

        return Ok(result);
    }

    [HttpDelete("{groupName}")]
    public async Task<IActionResult> Delete(
        string groupName)
    {
        await _groupService.DeleteAsync(groupName);

        return NoContent();
    }

    [HttpPost("{groupName}/members")]
    public async Task<IActionResult> AddMember(
        string groupName,
        [FromBody] UpdateGroupMemberRequest request)
    {
        await _groupService.AddMemberAsync(
            groupName,
            request.Username);

        return Ok(new
        {
            success = true,
            message = "Member added successfully."
        });
    }

    [HttpDelete("{groupName}/members/{username}")]
    public async Task<IActionResult> RemoveMember(
        string groupName,
        string username)
    {
        await _groupService.RemoveMemberAsync(
            groupName,
            username);

        return Ok(new
        {
            success = true,
            message = "Member removed successfully."
        });
    }
}