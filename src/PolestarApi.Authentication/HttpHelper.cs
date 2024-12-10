using System.Web;

namespace PolestarApi.Authentication;

/// <summary>
/// Provides utility methods for handling HTTP requests and responses in the Polestar authentication process.
/// </summary>
/// <remarks>
/// This class offers helper methods for common tasks such as sending POST requests, validating HTTP responses,
/// extracting query parameters from URIs, and generating random strings.
/// </remarks>
internal static class HttpHelper
{
    /// <summary>
    /// Sends a POST request with form-encoded data to the specified URI and returns the resulting request URI.
    /// </summary>
    /// <param name="client">
    /// An <see cref="HttpClient"/> instance used to send the request.
    /// </param>
    /// <param name="uri">
    /// The target URI for the POST request.
    /// </param>
    /// <param name="data">
    /// A dictionary containing the form data to be sent in the request body.
    /// </param>
    /// <returns>
    /// A <see cref="Uri"/> representing the request URI after the POST request is sent.
    /// </returns>
    /// <exception cref="HttpRequestException">
    /// Thrown if the request fails, the response indicates an unsuccessful status code,
    /// or the resulting request URI is unavailable.
    /// </exception>
    public static async Task<Uri> PostFormAndGetRequestUriAsync(
        this HttpClient client,
        string uri,
        Dictionary<string, string> data)
    {
        var content = new FormUrlEncodedContent(data);
        var response = await client.PostAsync(uri, content);
        response.EnsureSuccess("POST request failed.");

        var requestUri = response.RequestMessage?.RequestUri;
        if (requestUri is null)
        {
            throw new HttpRequestException(
                "The resulting request URI is unavailable. " +
                "This might indicate an issue with the request configuration or the server's response."
            );
        }

        return requestUri;
    }

    /// <summary>
    /// Ensures that the HTTP response has a successful status code.
    /// </summary>
    /// <param name="response">
    /// The <see cref="HttpResponseMessage"/> to validate.
    /// </param>
    /// <param name="defaultErrorMessage">
    /// A custom error message to include in the exception if the validation fails.
    /// </param>
    /// <exception cref="HttpRequestException">
    /// Thrown if the response indicates an unsuccessful status code.
    /// </exception>
    public static void EnsureSuccess(
        this HttpResponseMessage response,
        string defaultErrorMessage)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = response.Content.ReadAsStringAsync().Result;
        var detailedErrorMessage = !string.IsNullOrWhiteSpace(responseBody)
            ? $"{defaultErrorMessage}. Response content: {responseBody}"
            : defaultErrorMessage;

        throw new HttpRequestException(detailedErrorMessage, null, response.StatusCode);
    }

    /// <summary>
    /// Extracts a query parameter value from a given URI.
    /// </summary>
    /// <param name="uri">
    /// The <see cref="Uri"/> containing the query string.
    /// </param>
    /// <param name="paramName">
    /// The name of the query parameter to extract.
    /// </param>
    /// <returns>
    /// The value of the specified query parameter, or an empty string if the parameter is not found.
    /// </returns>
    public static string ExtractQueryParam(
        this Uri uri,
        string paramName)
        => HttpUtility
               .ParseQueryString(uri.Query)
               .Get(paramName)
           ?? string.Empty;

    /// <summary>
    /// Generates a random alphanumeric string of the specified length.
    /// </summary>
    /// <param name="length">
    /// The length of the random string to generate.
    /// </param>
    /// <returns>
    /// A randomly generated alphanumeric string.
    /// </returns>
    public static string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
