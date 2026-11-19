namespace UnfathomablePayLink.Interfaces;

using UnfathomablePayLink.Enums;
using UnfathomablePayLink.Models;

/// <summary>
/// Defines operations for delivering webhook notifications when payment states change.
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Sends a webhook notification to the business's configured webhook URL when a payment reaches a terminal state
    /// (<see cref="PaymentState.Approved"/> or <see cref="PaymentState.Rejected"/>).
    /// </summary>
    /// <param name="payment">The processed <see cref="Payment"/>, including its associated <see cref="Business"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous notification operation.</returns>
    Task NotifyPaymentProcessedAsync(Payment payment, CancellationToken cancellationToken = default);
}
