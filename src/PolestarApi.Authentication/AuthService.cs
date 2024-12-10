using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using PolestarApi.Contracts.Abstractions;
using PolestarApi.Contracts.Models;

namespace PolestarApi.Authentication;

/// <summary>
/// Provides functionality for authenticating users via the Polestar OAuth identity provider.
/// </summary>
/// <remarks>
/// This service implements the OAuth 2.0 Authorization Code Flow with PKCE:
/// 1. Initiates the authorization process with a unique state parameter and PKCE.
/// 2. Submits user login credentials.
/// 3. Handles consent confirmation if required.
/// 4. Retrieves an authorization code and exchanges it for an access token.
///
/// This service is specific to the Polestar identity provider and is not a generic OAuth implementation.
/// </remarks>
public sealed class AuthService : IAuthService
{
    private readonly HttpClient httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    public AuthService()
    {
        httpClient = new HttpClient(
            new HttpClientHandler
            {
                UseCookies = true
            });
    }

    /// <summary>
    /// Authenticates a user and retrieves an OAuth access token.
    /// </summary>
    /// <param name="request">
    /// An <see cref="AuthRequest"/> object containing the user's email and password.
    /// </param>
    /// <returns>
    /// An <see cref="AuthResponse"/> containing the access token and additional details,
    /// or <c>null</c> if authentication fails.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the authorization code cannot be obtained during the process.
    /// </exception>
    public async Task<AuthResponse?> Authenticate(AuthRequest request)
    {
        var state = HttpHelper.GenerateRandomString(16);

        try
        {
            var authUri = BuildAuthorizationUri(state, out var codeVerifier);
            var resumePath = await FetchAuthorizationResumePathAsync(authUri);

            var loginUri = BuildLoginUri(resumePath);
            var loginResponse = await SubmitLoginRequestAsync(loginUri, request);

            if (IsConsentConfirmationNeeded(loginResponse))
            {
                loginResponse.AuthCode = await ConfirmConsentAsync(resumePath, loginResponse.UserId!);
            }

            if (loginResponse.AuthCode is null)
            {
                throw new InvalidOperationException("The authorization code could not be retrieved.");
            }

            return await ExchangeCodeForToken(loginResponse.AuthCode, codeVerifier);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            return null;
        }
    }

    /// <summary>
    /// Determines if the consent confirmation step is needed based on the login response.
    /// </summary>
    /// <param name="loginResponse">
    /// A tuple containing the authorization code and user ID obtained during login.
    /// </param>
    /// <returns>
    /// <c>true</c> if consent confirmation is required; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsConsentConfirmationNeeded((string? AuthCode, string? UserId) loginResponse)
        => loginResponse.AuthCode is null &&
           loginResponse.UserId is not null;

    /// <summary>
    /// Fetches the "resume path" required for the next step in the OAuth flow.
    /// </summary>
    /// <param name="authUri">
    /// The URI used to initiate the OAuth authorization process.
    /// </param>
    /// <returns>
    /// A string representing the "resume path," extracted from the response URI's query parameters.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown if the HTTP request fails or the response indicates an error.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the "resume path" cannot be extracted from the response URI.
    /// </exception>
    private async Task<string> FetchAuthorizationResumePathAsync(string authUri)
    {
        var response = await httpClient.GetAsync(authUri);
        response.EnsureSuccess("Initial authentication attempt failed.");

        var requestUri = response.RequestMessage?.RequestUri ?? null;
        if (requestUri is null)
        {
            throw new InvalidOperationException("Unable to fetch authorization resume path.");
        }

        return requestUri.ExtractQueryParam("resumePath");
    }

    /// <summary>
    /// Submits login credentials and retrieves the authorization code or user ID.
    /// </summary>
    /// <param name="loginUri">
    /// The URI for submitting the login credentials.
    /// </param>
    /// <param name="authRequest">
    /// The user's login details encapsulated in an <see cref="AuthRequest"/>.
    /// </param>
    /// <returns>
    /// A tuple containing the authorization code and user ID.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown if the HTTP request fails.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if both the authorization code and user ID are missing.
    /// </exception>
    private async Task<(string? AuthCode, string? UserId)> SubmitLoginRequestAsync(
        string loginUri,
        AuthRequest authRequest)
    {
        var loginData = new Dictionary<string, string>
        {
            { "pf.username", authRequest.Email },
            { "pf.pass", authRequest.Password }
        };

        var requestUri = await httpClient.PostFormAndGetRequestUriAsync(loginUri, loginData);
        var userId = requestUri.ExtractQueryParam("uid");
        var authCode = requestUri.ExtractQueryParam("code");

        if (authCode is null && userId is null)
        {
            throw new InvalidOperationException(
                $"Login request failed: both 'authCode' and 'userId' are missing in the response. " +
                $"Login URI: {loginUri}, Email: {authRequest.Email}");
        }

        return (authCode, userId);
    }

    /// <summary>
    /// Constructs the URI for submitting login credentials during the OAuth flow.
    /// </summary>
    /// <param name="resumePath">
    /// The continuation URI for the login process, obtained from the authorization step.
    /// </param>
    /// <returns>
    /// A fully constructed URI for submitting login credentials.
    /// </returns>
    private static string BuildLoginUri(string resumePath) =>
        $"{Settings.OAuthUri}/as/{resumePath}/resume/as/authorization.ping?client_id={Settings.ClientId}";

    /// <summary>
    /// Constructs the URI required to initiate the OAuth 2.0 authorization flow with PKCE.
    /// </summary>
    /// <param name="state">
    /// A unique string generated for the OAuth flow to protect against CSRF attacks.
    /// </param>
    /// <param name="codeVerifier">
    /// An output parameter that will hold the generated code verifier used in the PKCE process.
    /// </param>
    /// <returns>
    /// A string representing the full authorization URI that can be used to redirect the user to
    /// the OAuth authorization server.
    /// </returns>
    /// <remarks>
    /// This method constructs a URI that includes all necessary query parameters to initiate the
    /// OAuth 2.0 authorization code flow with PKCE:
    /// - client_id: The client identifier for the application making the request.
    /// - redirect_uri: The URI to which the authorization code will be sent after authentication.
    /// - response_type: Specifies that the authorization code should be returned.
    /// - state: A random string generated to prevent CSRF attacks.
    /// - scope: The access permissions requested by the application (openid, profile, email, customer:attributes).
    /// - code_challenge: The base64 URL-safe encoded SHA256 hash of the code verifier, used in PKCE to mitigate
    ///                   interception attacks.
    /// - code_challenge_method: Specifies the hashing algorithm used for the code challenge (S256).
    /// </remarks>
    private static string BuildAuthorizationUri(string state, out string codeVerifier)
    {
        // Generate a random code verifier and its corresponding code challenge.
        codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);

        // Construct and return the full OAuth authorization URI.
        return
            $"{Settings.OAuthUri}/as/authorization.oauth2" +
            $"?client_id={Settings.ClientId}" +
            $"&redirect_uri={Settings.RedirectUri}" +
            $"&response_type=code" +
            $"&state={state}" +
            $"&scope=openid profile email customer:attributes" +
            $"&code_challenge={codeChallenge}" +
            $"&code_challenge_method=S256";
    }

    /// <summary>
    /// Generates a random code verifier used in the OAuth 2.0 Authorization Code Flow with PKCE.
    /// </summary>
    /// <returns>
    /// A base64 URL-safe encoded string representing the code verifier.
    /// </returns>
    private static string GenerateCodeVerifier()
    {
        var randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        return Convert
            .ToBase64String(randomBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// Generates the code challenge based on the provided code verifier.
    /// </summary>
    /// <param name="codeVerifier">
    /// The code verifier used in the OAuth 2.0 Authorization Code Flow with PKCE.
    /// </param>
    /// <returns>
    /// A base64 URL-safe encoded string representing the code challenge.
    /// </returns>
    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(codeVerifier));

        return Convert
            .ToBase64String(hash)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <summary>
    /// Confirms consent and retrieves the authorization code required for OAuth authentication.
    /// </summary>
    /// <param name="resumePath">
    /// The continuation URI for the consent step.
    /// </param>
    /// <param name="userId">
    /// The unique identifier for the user obtained during login.
    /// </param>
    /// <returns>
    /// The OAuth authorization code.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown if the HTTP request fails.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the authorization code cannot be retrieved.
    /// </exception>
    private async Task<string> ConfirmConsentAsync(string resumePath, string userId)
    {
        var confirmUri = $"{Settings.OAuthUri}/as/{resumePath}/resume/as/authorization.ping";
        var confirmData = new Dictionary<string, string>
        {
            { "pf.submit", "false" },
            { "subject", userId }
        };

        var requestUri = await httpClient.PostFormAndGetRequestUriAsync(confirmUri, confirmData);
        var authCode = requestUri.ExtractQueryParam("code");

        if (authCode is null)
        {
            throw new InvalidOperationException(
                $"Failed to extract authorization code from the response. " +
                $"Resume path: {resumePath}, User ID: {userId}, Confirm URI: {confirmUri}");
        }

        return authCode;
    }

    /// <summary>
    /// Exchanges the authorization code for an access token.
    /// </summary>
    /// <param name="authCode">
    /// The authorization code obtained from the OAuth authorization flow.
    /// </param>
    /// <param name="codeVerifier">
    /// The code verifier used during the authorization process (PKCE).
    /// </param>
    /// <returns>
    /// An <see cref="AuthResponse"/> containing the access token and additional details.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown if the HTTP request fails during the token exchange process.
    /// </exception>
    private async Task<AuthResponse?> ExchangeCodeForToken(
        string authCode,
        string codeVerifier)
    {
        var tokenRequestData = new Dictionary<string, string>
        {
            { "grant_type", "authorization_code" },
            { "code", authCode },
            { "code_verifier", codeVerifier },
            { "client_id", Settings.ClientId },
            { "redirect_uri", Settings.RedirectUri }
        };

        var response = await httpClient
            .PostAsync(
                requestUri: $"{Settings.OAuthUri}/as/token.oauth2",
                content: new FormUrlEncodedContent(tokenRequestData));

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        return JsonSerializer.Deserialize<AuthResponse>(responseContent, options);
    }
}
