using System.ComponentModel.DataAnnotations;

namespace PolestarApi.Contracts.Models;

/// <summary>
/// Represents an authentication request containing the user's credentials.
/// </summary>
public sealed record AuthRequest
{
    /// <summary>
    /// Gets or initializes the email address of the user.
    /// </summary>
    /// <remarks>
    /// This property is required and must be a valid email address format.
    /// </remarks>
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
    
    /// <summary>
    /// Gets or initializes the password of the user.
    /// </summary>
    /// <remarks>
    /// This property is required and must not be null or empty.
    /// </remarks>
    [Required]
    public string Password { get; init; } = string.Empty;
}