namespace UnfathomablePayLink.Interfaces;

using UnfathomablePayLink.Enums;
using UnfathomablePayLink.Exceptions;
using UnfathomablePayLink.Models;

/// <summary>
/// Defines payment processing and query operations.
/// </summary>
public interface IPaymentService
{
    /// <summary>
    /// Retrieves a payment transaction by its unique identifier.
    /// </summary>
    /// <param name="transactionId">The payment transaction identifier.</param>
    /// <returns>
    /// The matching <see cref="Payment"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<Payment?> GetPaymentByIdAsync(int transactionId);

    /// <summary>
    /// Retrieves payment transactions for a business, optionally filtered by invoice and state.
    /// </summary>
    /// <param name="businessId">The identifier of the owning business.</param>
    /// <param name="invoiceId">Optional invoice identifier to filter by.</param>
    /// <param name="state">Optional <see cref="PaymentState"/> to filter by.</param>
    /// <returns>A list of matching <see cref="Payment"/> transactions.</returns>
    Task<List<Payment>> GetPaymentsAsync(int businessId, string? invoiceId = null, PaymentState? state = null);

    /// <summary>
    /// Validates business and invoice eligibility, creates a pending payment, and initiates asynchronous processing.
    /// </summary>
    /// <param name="businessId">The identifier of the business initiating the payment.</param>
    /// <param name="invoiceId">The identifier of the invoice to pay.</param>
    /// <returns>The newly created <see cref="Payment"/> in <see cref="PaymentState.Pending"/> state.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="invoiceId"/> is empty or whitespace.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the business or invoice cannot be found.</exception>
    /// <exception cref="PaymentFinishedException">Thrown when an approved payment already exists for the invoice.</exception>
    /// <exception cref="PaymentPendingException">Thrown when a pending payment already exists for the invoice.</exception>
    Task<Payment> ProcessPaymentAsync(int businessId, string invoiceId);
}
