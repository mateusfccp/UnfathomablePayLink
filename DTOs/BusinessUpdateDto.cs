namespace UnfathomablePayLink.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Data transfer object for updating an existing business entity. The existing API key is always preserved.
/// </summary>
public class BusinessUpdateDto
{
    /// <summary>
    /// Gets or sets the updated display or legal name of the business.
    /// </summary>
    [Required(ErrorMessage = "Business name is required.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Business name must be between 1 and 100 characters.")]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the updated unique tax identification document (e.g., CUIT) of the business.
    /// </summary>
    [Required(ErrorMessage = "Business document is required.")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "Business document must be between 1 and 20 characters.")]
    public required string Document { get; set; }

    /// <summary>
    /// Gets or sets the optional absolute HTTP or HTTPS webhook URL. Pass <see langword="null"/> or empty to clear it.
    /// </summary>
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Gets or sets the optional invoice provider key. Pass <see langword="null"/> to preserve the current provider.
    /// </summary>
    public string? InvoiceProviderKey { get; set; }
}
