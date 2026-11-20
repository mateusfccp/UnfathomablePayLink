namespace UnfathomablePayLink.DTOs;

/// <summary>
/// Data transfer object representing a registered business returned by administrator endpoints.
/// </summary>
public class BusinessDto
{
    /// <summary>
    /// Gets or sets the unique database identifier of the business.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display or legal name of the business.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the unique tax identification document (e.g., CUIT) of the business.
    /// </summary>
    public string Document { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the server-generated API key used by this business to authenticate requests.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional webhook URL notified when payments reach a terminal state.
    /// </summary>
    public string? WebhookUrl { get; set; }

    /// <summary>
    /// Gets or sets the key of the invoice provider configured for this business.
    /// </summary>
    public string InvoiceProviderKey { get; set; } = string.Empty;
}
