namespace UnfathomablePayLink.Options;

/// <summary>
/// Configuration options for integrating with the external Gobernanza Digital API.
/// </summary>
public sealed class GobernanzaDigitalOptions
{
    /// <summary>
    /// The configuration section name for Gobernanza Digital settings.
    /// </summary>
    public const string SectionName = "GobernanzaDigital";

    /// <summary>
    /// Gets or sets the base URL of the Gobernanza Digital API.
    /// </summary>
    public string BaseUrl { get; set; } = "https://localhost:7233";
}
