using PolestarApi.Contracts.Models;

namespace PolestarApi.Authentication;

public interface IGraphQLService
{
    Task<AuthResponse> GetAuthTokenAsync(string code);
}