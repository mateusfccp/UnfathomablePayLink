using UnfathomablePayLink.Exceptions;
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
    /// Creates a new payment for the given business and invoice.
    /// </summary>
    /// <exception cref="PaymentPendingException">
    /// Thrown when there's already a pending payment to the given invoice.
    /// </exception>
    /// <exception cref="PaymentFinishedException">
    /// Thrown when there's already a finished payment to the given invoice.
    /// </exception>
    Task CreatePaymentForInvoiceId(int business, String invoiceId);

    /// <summary>
    /// Adds a new payment to the database.
    /// </summary>
    /// <param name="payment">The payment entity to add.</param>
    Task<Payment> AddPaymentAsync(Payment payment);
}
