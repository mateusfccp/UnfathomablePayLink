namespace UnfathomablePayLink.DTOs;

using UnfathomablePayLink.Enums;

/// <summary>
/// Data transfer object representing a payment transaction returned by the API and webhook payloads.
/// </summary>
public class PaymentDto
{
    /// <summary>
    /// Gets or sets the unique identifier of the payment.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the external identifier of the paid invoice.
    /// </summary>
    public string InvoiceId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the monetary amount of the payment.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="PaymentState"/> of the payment.
    /// </summary>
    public PaymentState State { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the payment was registered.
    /// </summary>
    public DateTime? PaymentDate { get; set; }
}
