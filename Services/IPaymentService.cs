namespace UnfathomablePayLink.Services;

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
    /// Retrieves all payment attempts for a given business and invoice.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="invoiceId">The invoice code.</param>
    Task<List<Payment>> GetPaymentsByInvoiceAsync(int businessId, string invoiceId);

    /// <summary>
    /// Retrieves the most recent payment for a given business and invoice.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="invoiceId">The invoice code.</param>
    Task<Payment?> GetLatestPaymentByInvoiceAsync(int businessId, string invoiceId);

    /// <summary>
    /// Retrieves the most recent payment associated with an invoice code across businesses.
    /// </summary>
    /// <param name="invoiceId">The invoice code.</param>
    Task<Payment?> GetPaymentByInvoiceIdAsync(string invoiceId);

    /// <summary>
    /// Processes and records a payment for a business invoice.
    /// Validates business existence, invoice state, and amount.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="invoiceId">The invoice code.</param>
    /// <param name="amount">The optional amount to pay. If omitted, it is queried from the external invoice.</param>
    Task<Payment> ProcessPaymentAsync(int businessId, string invoiceId, decimal? amount = null);

    /// <summary>
    /// Queries the external invoice details from the business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="invoiceId">The invoice code.</param>
    Task<Invoice> ConsultInvoiceAsync(int businessId, string invoiceId);
}
