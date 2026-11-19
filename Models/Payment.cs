namespace UnfathomablePayLink.Models;

using UnfathomablePayLink.Enums;

/// <summary>
/// Represents a payment transaction for a <see cref="Models.Business"/> invoice.
/// </summary>
sealed public class Payment
{
    /// <summary>
    /// Gets or sets the unique database identifier of the payment.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="Models.Business"/> that owns this payment.
    /// </summary>
    public required Business Business { get; set; }

    /// <summary>
    /// Gets or sets the external identifier of the invoice being paid.
    /// </summary>
    public required string InvoiceId { get; set; }

    /// <summary>
    /// Gets or sets the total monetary amount of the payment.
    /// </summary>
    public required decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets the current processing <see cref="PaymentState"/> of the payment.
    /// </summary>
    public required PaymentState State { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the payment was created or processed.
    /// </summary>
    public DateTime? PaymentDate { get; set; }
}
