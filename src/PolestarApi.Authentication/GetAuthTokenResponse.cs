using System.Text.Json.Serialization;

namespace PolestarApi.Authentication;

/// <summary>
/// Represents the root response structure for the GraphQL query to retrieve an authentication token.
/// </summary>
public sealed record GetAuthTokenResponse
{
    /// <summary>
    /// Contains the data returned by the GraphQL query.
    /// </summary>
    [JsonPropertyName("data")]
    public GetAuthTokenData Data { get; init; } = default!;
}
