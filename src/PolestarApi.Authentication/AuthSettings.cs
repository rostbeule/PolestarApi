namespace PolestarApi.Authentication;

/// <summary>
/// Contains authentication-related settings and constants for the Polestar API.
/// </summary>
public static class AuthSettings
{
    /// <summary>
    /// The client ID used for OAuth authentication with the Polestar API.
    /// </summary>
    public const string ClientId = "l3oopkc_10";

    /// <summary>
    /// The base URI for the Polestar OAuth authentication service.
    /// </summary>
    public const string OAuthUri = "https://polestarid.eu.polestar.com";

    /// <summary>
    /// The redirect URI used after successful authentication with the Polestar API.
    /// </summary>
    public const string RedirectUri = "https://www.polestar.com/sign-in-callback";
}
