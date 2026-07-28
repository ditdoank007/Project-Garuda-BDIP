using BDIP.Application.ImportGroups;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/groups/import")]
public class GroupImportController : ControllerBase
{
    private readonly IGroupImportService _service;

    public GroupImportController(
        IGroupImportService service)
    {
        _service = service;
    }

    [HttpPost("preview")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Preview(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "CSV file is required."
            });
        }

        await using var stream = file.OpenReadStream();

        var result = await _service.PreviewAsync(
            stream,
            cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Preview generated successfully.",
            data = result
        });
    }

    [HttpPost("execute")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Execute(
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "CSV file is required."
            });
        }

        await using var stream = file.OpenReadStream();

        var result = await _service.ImportAsync(
            stream,
            cancellationToken);

        return Ok(new
        {
            success = true,
            message = "Import completed.",
            data = result
        });
    }
}