using PolestarApi.Contracts.Abstractions;
using PolestarApi.Contracts.Models;

namespace PolestarApi.Authentication;

/// <summary>
/// Provides functionality for authenticating users via the Polestar OAuth identity provider.
/// </summary>
/// <remarks>
/// This service implements the OAuth 2.0 authorization code flow:
/// 1. Initiates the authorization process with a unique state parameter.
/// 2. Submits user login credentials.
/// 3. Handles consent confirmation if required.
/// 4. Retrieves an authorization code to exchange for an access token.
///
/// ### Dependencies:
/// - <see cref="HttpClient"/>: Used for HTTP requests.
/// - <see cref="GraphqlService"/>: Facilitates communication with Polestar's GraphQL API.
///
/// This service is specific to the Polestar identity provider and is not a generic OAuth implementation.
/// </remarks>
public sealed class AuthService : IAuthService
{
    private readonly HttpClient httpClient;
    private readonly GraphqlService graphqlService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <remarks>
    /// Configures an <see cref="HttpClient"/> to handle cookies for session management
    /// and initializes the <see cref="GraphqlService"/> for token management.
    /// </remarks>
    public AuthService()
    {
        httpClient = new HttpClient(
            new HttpClientHandler
            {
                UseCookies = true
            });

        graphqlService = new GraphqlService(httpClient);
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
            var authUri = BuildAuthorizationUri(state);
            var resumePath = await FetchAuthorizationResumePathAsync(authUri);

            var loginUri = BuildLoginUri(resumePath);
            var loginResponse = await SubmitLoginRequestAsync(loginUri, request);

            if (IsConsentConfirmationNeeded(loginResponse))
            {
                loginResponse.AuthCode = await ConfirmConsentAsync(resumePath, loginResponse.UserId!);
            }

            return await graphqlService.GetAuthTokenAsync(loginResponse.AuthCode!);
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
    /// Constructs the OAuth authorization URI with the required parameters.
    /// </summary>
    /// <param name="state">
    /// A unique state string used to correlate the request and prevent CSRF attacks.
    /// </param>
    /// <returns>
    /// A fully constructed URI for initiating the OAuth authorization process.
    /// </returns>
    private static string BuildAuthorizationUri(string state) =>
        $"{Settings.OAuthUri}/as/authorization.oauth2" +
        $"?client_id={Settings.ClientId}" +
        $"&redirect_uri={Settings.RedirectUri}" +
        $"&response_type=code" +
        $"&state={state}" +
        $"&scope=openid profile email customer:attributes";

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
}
