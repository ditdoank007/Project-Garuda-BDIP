using BDIP.Infrastructure.LDAP;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.Protocols;

namespace BDIP.API.Controllers;

[ApiController]
[Route("api/ldap")]
public class LdapTestController : ControllerBase
{
    private readonly ILdapConnectionFactory _factory;

    public LdapTestController(ILdapConnectionFactory factory)
    {
        _factory = factory;
    }

    [HttpGet("test")]
    public IActionResult Test()
    {
        try
        {
            using var connection = _factory.Create();

            var request = new SearchRequest(
                "ou=People,dc=basarnas,dc=go,dc=id",
                "(objectClass=inetOrgPerson)",
                SearchScope.Subtree,
                new[]
                {
                    "uid",
                    "cn",
                    "mail"
                });

            var response = (SearchResponse)connection.SendRequest(request);

            var users = response.Entries
                .Cast<SearchResultEntry>()
                .Select(x => new
                {
                    Username = x.Attributes["uid"]?[0]?.ToString(),
                    Name = x.Attributes["cn"]?[0]?.ToString(),
                    Email = x.Attributes["mail"]?[0]?.ToString()
                });

            return Ok(new
            {
                success = true,
                total = response.Entries.Count,
                users
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                success = false,
                message = ex.Message,
                inner = ex.InnerException?.Message,
                stack = ex.StackTrace
            });
        }
    }
}
