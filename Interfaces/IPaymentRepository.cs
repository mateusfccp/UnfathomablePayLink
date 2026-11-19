using UnfathomablePayLink.Enums;
using UnfathomablePayLink.Models;

namespace UnfathomablePayLink.Interfaces;

/// <summary>
/// Interface <c>IInvoiceRepository</c> defines the interface of a payment
/// repository.
/// <para>
/// The interface describes the repository that provides the creation and update
/// operations of payments in the system.
/// </para>
/// </summary>
public interface IPaymentRepository
{
    /// <summary>
    /// Gets the payment associated with given ID.
    /// </summary>
    /// <param name="id">The unique ID of the payment in the system.</param>
    /// <returns>The payment associated with the given ID, or null if not found.</returns>
    Task<Payment?> GetPaymentById(int id);

    /// <summary>
    /// Gets all the payments associated with the given business and invoice.
    /// <para>
    /// It may return more than one payment because an invoice may have
    /// previously rejected payments, but it should have at most one approved or
    /// pending payment.
    /// </para>
    /// </summary>
    /// <param name="businessId">
    /// The ID of the business from which to get the payments.
    /// </param>
    /// <param name="invoiceId">
    /// The ID of the invoice in the associated business.
    /// </param>
    Task<List<Payment>> GetPaymentsByInvoiceId(int businessId, String invoiceId);

    /// <summary>
    /// Gets the last payment associated with the given business and invoice.
    /// </summary>
    /// <param name="businessId">
    /// The ID of the business from which to get the payment.
    /// </param>
    /// <param name="invoiceId">
    /// The ID of the invoice in the associated business.
    /// </param>
    Task<Payment?> GetLatestPaymentByInvoiceId(int businessId, String invoiceId);

    /// <summary>
    /// Retrieves all payments in the system.
    /// </summary>
    Task<List<Payment>> GetAllPaymentsAsync();

    /// <summary>
    /// Retrieves all payments associated with a given business.
    /// </summary>
    /// <param name="businessId">The business identifier.</param>
    Task<List<Payment>> GetPaymentsByBusinessAsync(int businessId);

    /// <summary>
    /// Adds a new payment to the database.
    /// </summary>
    /// <param name="payment">The payment entity to add.</param>
    Task<Payment> AddPaymentAsync(Payment payment);

    /// <summary>
    /// Updates a payment's state.
    /// </summary>
    /// <param name="paymentId">The payment ID to update.</param>
    /// <param name="state">The new state of this payment.</param>
    /// <returns>The updated payment including its associated <see cref="Business"/>.</returns>
    Task<Payment> UpdatePaymentStateAsync(int paymentId, PaymentState state);
}
