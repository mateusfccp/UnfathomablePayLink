namespace UnfathomablePayLink.Exceptions;

/// <summary>
/// An exception thrown when a payment creation was attempted but there's
/// already a pending payment.
/// </summary>
public class PaymentPendingException : Exception
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
    /// Creates a <c>PaymentPendingException</c>.
    /// </summary>
    public PaymentPendingException(int businessId, string invoiceId) {
        this.businessId = businessId;
        this.invoiceId = invoiceId;
    }
}
