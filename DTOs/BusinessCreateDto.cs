namespace UnfathomablePayLink.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Data transfer object for creating a new business entity. The API key is generated automatically by the server.
/// </summary>
public class BusinessCreateDto
{
    /// <summary>
    /// Gets or sets the display or legal name of the business.
    /// </summary>
    [Required(ErrorMessage = "Business name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Business name must be between 1 and 100 characters.")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the unique tax identification document (e.g., CUIT) of the business.
    /// </summary>
    [Required(ErrorMessage = "Business document is required.")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "Business document must be between 1 and 20 characters.")]
    public required string Document { get; set; }

    /// <summary>
    /// Gets or sets the optional absolute HTTP or HTTPS webhook URL notified when payments complete.
    /// </summary>
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Gets or sets the optional invoice provider key (defaults to <c>mock</c> when omitted).
    /// </summary>
    public string? InvoiceProviderKey { get; set; }
}
