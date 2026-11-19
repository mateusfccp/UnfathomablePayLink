namespace UnfathomablePayLink.Interfaces;

using UnfathomablePayLink.Enums;
using UnfathomablePayLink.Models;

/// <summary>
/// Service interface defining payment processing and query operations.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Retrieves a payment transaction by its ID.
    /// </summary>
    /// <param name="transactionId">The payment ID.</param>
    Task<Payment?> GetPaymentByIdAsync(int transactionId);

    /// <summary>
    /// Retrieves all recorded payment transactions.
    /// </summary>
    /// <param name="businessId"></param>
    /// <param name="invoiceId"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    Task<List<Payment>> GetPaymentsAsync(int businessId, string? invoiceId = null, PaymentState? state = null);

    /// <summary>
    /// Processes and records a payment for a business invoice.
    /// Validates business existence, invoice state, and amount.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="invoiceId">The invoice code.</param>
    Task<Payment> ProcessPaymentAsync(int businessId, string invoiceId);
}
