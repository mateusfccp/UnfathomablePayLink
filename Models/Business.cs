using Microsoft.EntityFrameworkCore;

namespace UnfathomablePayLink.Models;

/// <summary>
/// Represents a merchant or business registered in PayLink to process <see cref="Payment"/> transactions.
/// </summary>
[Index(nameof(Document), IsUnique = true)]
[Index(nameof(ApiKey), IsUnique = true)]
sealed public class Business
{
    /// <summary>
    /// Gets or sets the unique database identifier of the business.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the display or legal name of the business.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the unique tax identification document (e.g., CUIT) of the business.
    /// </summary>
    public required string Document { get; set; }

    /// <summary>
    /// Gets or sets the unique API key used to authenticate requests from this business.
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the optional webhook URL notified when payments reach a terminal state.
    /// </summary>
    public string? WebhookUrl { get; set; }
}
