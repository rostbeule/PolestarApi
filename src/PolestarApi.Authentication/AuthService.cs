using PolestarApi.Contracts.Abstractions;
using PolestarApi.Contracts.Models;

namespace PolestarApi.Authentication;

/// <summary>
/// Provides functionality for authenticating users via the Polestar OAuth identity provider.
/// </summary>
/// <remarks>
/// The service follows the OAuth 2.0 authorization code flow:
/// 1. Initiates the authorization process with a unique state parameter.
/// 2. Submits the user's login credentials.
/// 3. Handles consent confirmation if required.
/// 4. Retrieves the OAuth authorization code, which can be exchanged for an access token.
///
/// Dependencies:
/// - <see cref="HttpClient"/>: Used for sending HTTP requests during the authentication process.
/// - <see cref="GraphQLService"/>: Facilitates interaction with the Polestar GraphQL API to obtain access tokens.
///
/// The class is tightly coupled to the Polestar identity provider and is not designed for general OAuth use.
/// </remarks>
public sealed class AuthService : IAuthService
{
    private readonly HttpClient httpClient;
    private readonly GraphQLService graphQLService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthService"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor sets up the required dependencies for the authentication service:
    /// - Configures an <see cref="HttpClient"/> with a handler to manage cookies, enabling session management.
    /// - Creates an instance of <see cref="GraphQLService"/> for interacting with the Polestar GraphQL API.
    /// </remarks>
    public AuthService()
    {
        httpClient = new HttpClient(new HttpClientHandler { UseCookies = true });
        graphQLService = new GraphQLService(httpClient);
    }

    /// <summary>
    /// Authenticates a user and retrieves an OAuth access token.
    /// </summary>
    /// <param name="request">
    /// An <see cref="AuthRequest"/> object containing the user's email and password.
    /// </param>
    /// <returns>
    /// An <see cref="AuthResponse"/> object containing the access token and other details, or null if authentication fails.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the authorization code could not be extracted during the process.
    /// </exception>
    public async Task<AuthResponse?> Authenticate(AuthRequest request)
    {
        var authorizationCode = await FetchOAuthAuthorizationCodeAsync(request);
        return await graphQLService.GetAuthTokenAsync(authorizationCode);
    }

    /// <summary>
    /// Executes the steps required to obtain an OAuth authorization code from the Polestar identity provider.
    /// </summary>
    /// <remarks>
    /// The process includes:
    /// 1. Generating a unique state string to prevent CSRF attacks.
    /// 2. Initiating the authorization process to retrieve a resume path.
    /// 3. Submitting the user's login credentials to obtain the authorization code.
    ///
    /// If consent confirmation is required, this step is handled within the login submission process.
    /// </remarks>
    /// <param name="request">
    /// An <see cref="AuthRequest"/> object containing the user's email and password for authentication.
    /// </param>
    /// <returns>
    /// A string representing the OAuth authorization code.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the authorization code could not be extracted during the process.
    /// </exception>
    private async Task<string> FetchOAuthAuthorizationCodeAsync(AuthRequest request)
    {
        var state = HttpHelper.GenerateRandomString(16);
        var resumePath = await FetchAuthorizationResumePathAsync(state);
        var authorizationCode = await SubmitLoginAsync(resumePath, request);

        if (string.IsNullOrEmpty(authorizationCode))
        {
            throw new InvalidOperationException("Authorization code could not be extracted.");
        }

        return authorizationCode;
    }

    /// <summary>
    /// Fetches the "resume path" required to continue the OAuth authorization process.
    /// </summary>
    /// <param name="state">
    /// A unique state string used to correlate the request and prevent CSRF attacks.
    /// </param>
    /// <returns>
    /// A string representing the "resume path," which is essential for proceeding with the login process.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown if the HTTP request fails or the response indicates an unsuccessful status.
    /// </exception>

    private async Task<string> FetchAuthorizationResumePathAsync(string state)
    {
        var authUri = BuildAuthorizationUri(state);
        var response = await httpClient.GetAsync(authUri);
        HttpHelper.EnsureSuccess(response, "Initial authentication attempt failed.");
        return HttpHelper.ExtractQueryParam(response.RequestMessage.RequestUri, "resumePath");
    }

    /// <summary>
    /// Submits the user's login credentials and retrieves the OAuth authorization code.
    /// </summary>
    /// <param name="resumePath">
    /// The continuation URI for the login flow, obtained from the authorization step.
    /// </param>
    /// <param name="request">
    /// An <see cref="AuthRequest"/> object containing the user's email and password.
    /// </param>
    /// <returns>
    /// A string representing the OAuth authorization code, or an empty string if unsuccessful.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown if the HTTP request fails.
    /// </exception>
    private async Task<string> SubmitLoginAsync(string resumePath, AuthRequest request)
    {
        var loginUri = BuildLoginUri(resumePath);
        var loginData = new Dictionary<string, string>
        {
            { "pf.username", request.Email },
            { "pf.pass", request.Password }
        };

        var requestUri = await HttpHelper.PostFormAndGetRequestUriAsync(httpClient, loginUri, loginData);
        var userId = HttpHelper.ExtractQueryParam(requestUri, "uid");
        var authorizationCode = HttpHelper.ExtractQueryParam(requestUri, "code");

        if (string.IsNullOrEmpty(authorizationCode) && !string.IsNullOrEmpty(userId))
        {
            authorizationCode = await ConfirmConsentAsync(resumePath, userId);
        }

        return authorizationCode;
    }

    /// <summary>
    /// Confirms the user's consent and retrieves the OAuth authorization code.
    /// </summary>
    /// <param name="resumePath">
    /// The continuation URI for the consent step.
    /// </param>
    /// <param name="uid">
    /// The unique user identifier returned during the login process.
    /// </param>
    /// <returns>
    /// A string representing the OAuth authorization code.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown if the HTTP request fails.
    /// </exception>
    private async Task<string> ConfirmConsentAsync(string resumePath, string uid)
    {
        var confirmUri = $"{AuthSettings.OAuthUri}/as/{resumePath}/resume/as/authorization.ping";
        var confirmData = new Dictionary<string, string>
        {
            { "pf.submit", "true" },
            { "subject", uid }
        };

        var requestUri = await HttpHelper.PostFormAndGetRequestUriAsync(httpClient, confirmUri, confirmData);
        return HttpHelper.ExtractQueryParam(requestUri, "code");
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
        $"{AuthSettings.OAuthUri}/as/authorization.oauth2" +
        $"?client_id={AuthSettings.ClientId}" +
        $"&redirect_uri={AuthSettings.RedirectUri}" +
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
        $"{AuthSettings.OAuthUri}/as/{resumePath}/resume/as/authorization.ping?client_id={AuthSettings.ClientId}";
}
