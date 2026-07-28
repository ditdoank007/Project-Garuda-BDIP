using BDIP.Application.ImportUsers;
using Microsoft.AspNetCore.Mvc;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/users/import/synology")]
public class UserImportController : ControllerBase
{
    private readonly ISynologyUserCsvParser _parser;

    public UserImportController(
        ISynologyUserCsvParser parser)
    {
        _parser = parser;
    }

    [HttpPost("preview")]
    [RequestSizeLimit(10_000_000)]
    public async Task<IActionResult> Preview(
        [FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new
            {
                success = false,
                message = "CSV file is required."
            });
        }

        if (!file.FileName.EndsWith(
            ".csv",
            StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                success = false,
                message = "Only CSV files are supported."
            });
        }

        await using var stream = file.OpenReadStream();

        var result = await _parser.PreviewAsync(stream);

        return Ok(new
        {
            success = true,
            message = "Synology user CSV preview generated successfully.",
            data = result
        });
    }
}
