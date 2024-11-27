using System.Net.Mime;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolestarApi.Contracts.Abstractions;
using PolestarApi.Contracts.Models;

namespace PolestarApi.App.Controller;

/// <summary>
/// Controller responsible for handling authentication-related operations.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(
    IAuthService service)
    : ControllerBase
{
    /// <summary>
    /// Authenticate
    /// </summary>
    /// <param name="request">
    /// The authentication request containing the user's credentials.
    /// </param>
    /// <remarks>
    /// Example request:
    /// <code>
    /// {
    ///   "email": "user@example.com",
    ///   "password": "P@ssw0rd123"
    /// }
    /// </code>
    /// </remarks>
    /// <response code="200">Returns the authentication token and refresh token.</response>
    /// <response code="400">If the request is invalid (e.g., missing required fields).</response>
    /// <response code="401">If the credentials are invalid or authorization failed.</response>
    [HttpPost]
    [AllowAnonymous]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Authenticate([FromBody] AuthRequest request)
    {
        var response = await service.Authenticate(request);

        if (response is null)
        {
            return Unauthorized();
        }

        return Ok(response);
    }
}
