namespace UnfathomablePayLink.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Data transfer object for configuring or updating a business's webhook URL.
/// </summary>
public class WebhookUpdateDto
{
    /// <summary>
    /// Gets or sets the absolute HTTP or HTTPS URL to notify upon payment completion.
    /// </summary>
    [Required(ErrorMessage = "Webhook URL is required.")]
    public required string Url { get; set; }
}
