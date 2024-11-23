using PolestarApi.Contracts.Abstractions;
using PolestarApi.Contracts.Models;

namespace PolestarApi.Authentication;

/// <summary>
/// Provides an implementation of the <see cref="IAuthService"/> interface for user authentication.
/// </summary>
public class AuthService : IAuthService
{
    /// <inheritdoc/>
    public async Task<AuthResponse?> Authenticate(AuthRequest request)
    {
        await Task.Delay(1000);
        return new AuthResponse
        {
            IdToken = "id_token",
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            ExpiresIn = 123
        };
    }
}