using System.Text;
using System.Text.Json;
using PolestarApi.Contracts.Models;

namespace PolestarApi.Authentication;

public class GraphQLService : IGraphQLService
{
    private readonly HttpClient httpClient;
    private const string ApiAuthURI = "https://pc-api.polestar.com/eu-north-1/auth";

    public GraphQLService(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<AuthResponse> GetAuthTokenAsync(string code)
    {
        const string query = @"
        query getAuthToken($code: String!) {
            getAuthToken(code: $code) {
                id_token
                access_token
                refresh_token
                expires_in
            }
        }";

        var requestBody = new
        {
            query,
            variables = new { code },
            operationName = "getAuthToken"
        };

        string jsonBody = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync(ApiAuthURI, content);
        EnsureSuccess(response, "Fehler beim Abrufen des Tokens über GraphQL.");

        string responseBody = await response.Content.ReadAsStringAsync();
        return DeserializeResponse(responseBody);
    }
    
    private static void EnsureSuccess(HttpResponseMessage response, string errorMessage)
    {
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(errorMessage);
        }
    }
    
    private static AuthResponse DeserializeResponse(string responseBody)
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(responseBody));
        var tokenResponse = new AuthResponse();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();
                switch (propertyName)
                {
                    case "id_token":
                        tokenResponse.IdToken = reader.GetString();
                        break;
                    case "access_token":
                        tokenResponse.AccessToken = reader.GetString();
                        break;
                    case "refresh_token":
                        tokenResponse.RefreshToken = reader.GetString();
                        break;
                    case "expires_in":
                        tokenResponse.ExpiresIn = reader.GetInt32();
                        break;
                }
            }
        }

        return tokenResponse;
    }
}