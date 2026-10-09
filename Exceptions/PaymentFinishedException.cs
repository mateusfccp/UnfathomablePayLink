namespace UnfathomablePayLink.Exceptions;

/// <summary>
/// An exception thrown when a payment creation was attempted but there's
/// already an accepted payment.
/// </summary>
public class PaymentFinishedException : Exception
{
    /// <summary>
    /// The business ID to which the invoice belong.
    /// </summary>
    public int businessId { get; }

    /// <summary>
    /// The internal invoice ID in its business.
    /// </summary>
    public string invoiceId { get; }

    /// <summary>
    /// Creates a <c>PaymentFinishedException</c>.
    /// </summary>
    public PaymentFinishedException(int businessId, string invoiceId) {
        this.businessId = businessId;
        this.invoiceId = invoiceId;
    }
}
