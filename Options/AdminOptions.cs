namespace UnfathomablePayLink.Options;

/// <summary>
/// Configuration options for administrator authentication.
/// </summary>
public sealed class AdminOptions
{
    /// <summary>
    /// The configuration section name for administrator settings.
    /// </summary>
    public const string SectionName = "Admin";

    /// <summary>
    /// Gets or sets the exclusive API key used to authenticate administrator requests via <c>X-Api-Key</c>.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
}
