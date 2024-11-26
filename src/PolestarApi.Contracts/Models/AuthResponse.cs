namespace PolestarApi.Contracts.Models;

/// <summary>
/// Represents the response data for an <see cref="AuthRequest"/>.
/// </summary>
public sealed class AuthResponse
{
    /// <summary>
    /// Gets or initializes the ID token.
    /// </summary>
    public string IdToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or initializes the access token.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or initializes the refresh token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or initializes the duration (in seconds) until the token expires.
    /// </summary>
    public int ExpiresIn { get; set; }
}