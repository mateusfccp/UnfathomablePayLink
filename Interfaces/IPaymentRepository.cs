namespace UnfathomablePayLink.Interfaces;

using UnfathomablePayLink.Enums;
using UnfathomablePayLink.Models;

/// <summary>
/// Defines the data access contract for creating, querying, and updating <see cref="Payment"/> entities.
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Retrieves a payment by its unique identifier, including its associated <see cref="Business"/>.
    /// </summary>
    /// <param name="id">The unique identifier of the payment.</param>
    /// <returns>
    /// The matching <see cref="Payment"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<Payment?> GetPaymentById(int id);

    /// <summary>
    /// Retrieves all payments associated with the specified business and invoice.
    /// </summary>
    /// <remarks>
    /// An invoice may have multiple rejected attempts alongside at most one pending or approved payment.
    /// </remarks>
    /// <param name="businessId">The identifier of the business.</param>
    /// <param name="invoiceId">The identifier of the invoice within the business.</param>
    /// <returns>A list of matching <see cref="Payment"/> records.</returns>
    Task<List<Payment>> GetPaymentsByInvoiceId(int businessId, string invoiceId);

    /// <summary>
    /// Retrieves all payments belonging to a given business.
    /// </summary>
    /// <param name="businessId">The business identifier.</param>
    /// <returns>A list of <see cref="Payment"/> records for the specified business.</returns>
    Task<List<Payment>> GetPaymentsByBusinessAsync(int businessId);

    /// <summary>
    /// Persists a new payment entity to the database.
    /// </summary>
    /// <param name="payment">The payment entity to add.</param>
    /// <returns>The persisted <see cref="Payment"/> entity with its generated identifier.</returns>
    Task<Payment> AddPaymentAsync(Payment payment);

    /// <summary>
    /// Updates the processing state of an existing payment.
    /// </summary>
    /// <param name="paymentId">The identifier of the payment to update.</param>
    /// <param name="state">The new <see cref="PaymentState"/>.</param>
    /// <returns>The updated <see cref="Payment"/> including its associated <see cref="Business"/>.</returns>
    Task<Payment> UpdatePaymentStateAsync(int paymentId, PaymentState state);
}
