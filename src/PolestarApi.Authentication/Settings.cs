namespace PolestarApi.Authentication;

/// <summary>
/// Contains authentication-related settings and constants for the Polestar API.
/// </summary>
internal static class Settings
{
    /// <summary>
    /// The client ID used for OAuth authentication with the Polestar API.
    /// </summary>
    /// <remarks>
    /// This is a unique identifier assigned to the client application by the Polestar OAuth provider.
    /// It is used during the OAuth authorization process to identify the application making the request.
    /// </remarks>
    public const string ClientId = "l3oopkc_10";

    /// <summary>
    /// The base URI for the Polestar OAuth authentication service.
    /// </summary>
    /// <remarks>
    /// This URI serves as the entry point for all OAuth-related operations, such as initiating
    /// the authorization flow and handling consent steps. It points to the Polestar identity provider.
    /// </remarks>
    public const string OAuthUri = "https://polestarid.eu.polestar.com";

    /// <summary>
    /// The redirect URI used after successful authentication with the Polestar API.
    /// </summary>
    /// <remarks>
    /// This URI is registered with the Polestar OAuth provider and specifies where users are redirected
    /// after completing the authentication process. It must match the redirect URI configured in
    /// the client application's settings.
    /// </remarks>
    public const string RedirectUri = "https://www.polestar.com/sign-in-callback";

    /// <summary>
    /// The base URI for the Polestar API authentication endpoint used for GraphQL requests.
    /// </summary>
    /// <remarks>
    /// This URI serves as the entry point for all GraphQL-based authentication operations,
    /// such as retrieving OAuth tokens. It is specific to the Polestar API region "eu-north-1".
    /// </remarks>
    public const string GraphqlApiAuthUri = "https://pc-api.polestar.com/eu-north-1/auth";
}
