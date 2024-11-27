using System.Net.Mime;
using System.Text;
using System.Text.Json;
using PolestarApi.Contracts.Models;

namespace PolestarApi.Authentication;

/// <summary>
/// Provides functionality for interacting with the Polestar GraphQL API.
/// </summary>
/// <remarks>
/// This service is responsible for sending GraphQL queries to the Polestar API to retrieve
/// authentication tokens. It uses a strongly-typed approach to deserialize the response
/// into an <see cref="AuthResponse"/> object.
/// </remarks>
internal sealed class GraphqlService
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphqlService"/> class.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> instance used for HTTP requests.</param>
    public GraphqlService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    /// <summary>
    /// Sends a GraphQL query to retrieve an authentication token using the provided authorization code.
    /// </summary>
    /// <param name="authCode">The authorization code obtained during the OAuth flow.</param>
    /// <returns>
    /// An <see cref="AuthResponse"/> containing the ID token, access token, refresh token, and expiration time.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown if the HTTP request fails or the response indicates an unsuccessful status.
    /// </exception>
    public async Task<AuthResponse> GetAuthTokenAsync(string authCode)
    {
        var query = GetAuthTokenQuery();

        var requestBody = new
        {
            query,
            variables = new { code = authCode },
            operationName = "getAuthToken"
        };

        var jsonBody = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonBody, Encoding.UTF8, MediaTypeNames.Application.Json);

        var response = await httpClient.PostAsync(Settings.GraphqlApiAuthUri, content);
        response.EnsureSuccess("Failed to retrieve token via GraphQL.");

        var responseBody = await response.Content.ReadAsStringAsync();
        return DeserializeResponse(responseBody);
    }

    /// <summary>
    /// Retrieves the GraphQL query string for fetching an authentication token.
    /// </summary>
    /// <returns>The GraphQL query string.</returns>
    private static string GetAuthTokenQuery() =>
        """
        query getAuthToken($code: String!) {
            getAuthToken(code: $code) {
                id_token
                access_token
                refresh_token
                expires_in
            }
        }
        """;

    /// <summary>
    /// Deserializes the JSON response from the GraphQL API into an <see cref="AuthResponse"/> object.
    /// </summary>
    /// <param name="responseBody">The JSON response body as a string.</param>
    /// <returns>An <see cref="AuthResponse"/> object containing authentication token details.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the deserialization fails or the expected data is missing in the response.
    /// </exception>
    private static AuthResponse DeserializeResponse(string responseBody)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var deserializedResponse = JsonSerializer.Deserialize<GetAuthTokenResponse>(responseBody, options);

        if (deserializedResponse?.Data.GetAuthToken is null)
        {
            throw new InvalidOperationException("Failed to deserialize the authentication response.");
        }

        return deserializedResponse.Data.GetAuthToken;
    }
}
