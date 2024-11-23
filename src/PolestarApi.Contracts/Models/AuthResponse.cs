namespace PolestarApi.Contracts.Models;

/// <summary>
/// Represents the response data for an <see cref="AuthRequest"/>.
/// </summary>
public sealed record AuthResponse
{
    /// <summary>
    /// Gets or initializes the ID token.
    /// </summary>
    public string IdToken { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the access token.
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the refresh token.
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// Gets or initializes the duration (in seconds) until the token expires.
    /// </summary>
    public int ExpiresIn { get; init; }
}