using System.Text.Json.Serialization;
using PolestarApi.Contracts.Models;

namespace PolestarApi.Authentication;

/// <summary>
/// Represents the data section of the GraphQL response for retrieving an authentication token.
/// </summary>
public sealed record GetAuthTokenData
{
    /// <summary>
    /// Contains the authentication token details returned by the `getAuthToken` query.
    /// </summary>
    [JsonPropertyName("getAuthToken")]
    public AuthResponse GetAuthToken { get; init; } = default!;
}
