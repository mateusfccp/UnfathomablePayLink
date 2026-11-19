namespace UnfathomablePayLink.DTOs;

/// <summary>
/// Data transfer object representing a business's configured webhook URL.
/// </summary>
public class WebhookDto
{
    /// <summary>
    /// Gets or sets the configured webhook URL, or <see langword="null"/> if none is set.
    /// </summary>
    public string? Url { get; set; }
}
