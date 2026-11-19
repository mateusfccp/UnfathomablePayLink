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
    /// Retrieves all payments in the system.
    /// </summary>
    Task<List<Payment>> GetAllPaymentsAsync();

    Task<List<Payment>> GetPaymentsAsync(int businessId, string? invoiceId = null, PaymentState? state = null);

    /// <summary>
    /// Retrieves all payments associated with a given business.
    /// </summary>
    /// <param name="businessId">The business identifier.</param>
    Task<List<Payment>> GetPaymentsByBusinessAsync(int businessId);

    /// <summary>
    /// Processes and records a payment for a business invoice.
    /// Validates business existence, invoice state, and amount.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="invoiceId">The invoice code.</param>
    /// <param name="amount">The optional amount to pay. If omitted or zero, it is queried from the external invoice.</param>
    /// <param name="state">The payment state to assign (defaults to Approved).</param>
    Task<Payment> ProcessPaymentAsync(int businessId, string invoiceId, decimal? amount = null, PaymentState? state = null);

    /// <summary>
    /// Queries the external invoice details from the business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="invoiceId">The invoice code.</param>
    Task<Invoice> ConsultInvoiceAsync(int businessId, string invoiceId);
}
