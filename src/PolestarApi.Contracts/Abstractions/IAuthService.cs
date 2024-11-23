using PolestarApi.Contracts.Models;

namespace PolestarApi.Contracts.Abstractions;

/// <summary>
/// Defines methods for user authentication.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Authenticates a user based on the provided credentials.
    /// </summary>
    /// <param name="request">An <see cref="AuthRequest"/> containing the user's credentials.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an 
    /// <see cref="AuthResponse"/> object if the authentication is successful, or <c>null</c> if 
    /// authentication fails.
    /// </returns>
    Task<AuthResponse?> Authenticate(AuthRequest request);
}