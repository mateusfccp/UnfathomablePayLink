namespace UnfathomablePayLink.Options;

/// <summary>
/// Configuration options for integrating with the external StreamHub billing API.
/// </summary>
public sealed class StreamHubOptions
{
    /// <summary>
    /// The configuration section name for StreamHub settings.
    /// </summary>
    public const string SectionName = "StreamHub";

    /// <summary>
    /// Gets or sets the base URL of the StreamHub API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://streamhub-api-lgkh.onrender.com";

    /// <summary>
    /// Gets or sets the email address used to authenticate against <c>POST /users/login</c>.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password used to authenticate against <c>POST /users/login</c>.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the fallback monetary amount used when a StreamHub invoice payload omits an amount field.
    /// </summary>
    public decimal DefaultFallbackAmount { get; set; } = 0m;
}
