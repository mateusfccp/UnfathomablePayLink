namespace UnfathomablePayLink.Interfaces;

using UnfathomablePayLink.Models;

/// <summary>
/// Service interface for delivering webhook notifications when payment states change.
/// </summary>
public interface IWebhookService
{
    /// <summary>
    /// Sends a webhook notification to the business's configured webhook URL when a payment reaches a terminal state.
    /// </summary>
    /// <param name="payment">The processed payment, including its associated <see cref="Business"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task NotifyPaymentProcessedAsync(Payment payment, CancellationToken cancellationToken = default);
}
