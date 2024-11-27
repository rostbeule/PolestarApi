using System.Text.Json.Serialization;

namespace PolestarApi.Contracts.Models;

/// <summary>
/// Represents the response data for an authentication request.
/// </summary>
/// <param name="IdToken">The ID token issued by the authentication provider.</param>
/// <param name="AccessToken">The access token used for accessing protected resources.</param>
/// <param name="RefreshToken">The refresh token used to obtain new access tokens.</param>
/// <param name="ExpiresIn">The duration (in seconds) until the token expires.</param>
public record AuthResponse(
    [property: JsonPropertyName("id_token")] string IdToken,
    [property: JsonPropertyName("access_token")] string AccessToken,
    [property: JsonPropertyName("refresh_token")] string RefreshToken,
    [property: JsonPropertyName("expires_in")] int ExpiresIn);
