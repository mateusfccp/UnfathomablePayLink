namespace UnfathomablePayLink.Exceptions;

using UnfathomablePayLink.Models;

/// <summary>
/// Exception thrown when payment creation is attempted for an invoice that already has an approved <see cref="Payment"/>.
/// </summary>
public class PaymentFinishedException : Exception
{
    /// <summary>
    /// Gets the identifier of the <see cref="Business"/> to which the invoice belongs.
    /// </summary>
    public int businessId { get; }

    /// <summary>
    /// Gets the internal invoice identifier within its <see cref="Business"/>.
    /// </summary>
    public string invoiceId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentFinishedException"/> class.
    /// </summary>
    /// <param name="businessId">The identifier of the owning business.</param>
    /// <param name="invoiceId">The identifier of the invoice that has already been paid.</param>
    public PaymentFinishedException(int businessId, string invoiceId)
    {
        this.businessId = businessId;
        this.invoiceId = invoiceId;
    }
}
