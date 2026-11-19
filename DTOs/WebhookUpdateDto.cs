namespace UnfathomablePayLink.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Data transfer object for configuring or updating a business's webhook URL.
/// </summary>
public class WebhookUpdateDto
{
    [Required(ErrorMessage = "Webhook URL is required.")]
    public required string Url { get; set; }
}
