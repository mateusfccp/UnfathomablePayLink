namespace UnfathomablePayLink.Interfaces;

/// <summary>
/// Interface <c>IInvoiceRepository</c> defines the interface of an invoice
/// repository.
///
/// The interface describes the repository that gets the invoices of a
/// <c>Business</c> and its details, so that they may be used to generate a
/// <c>Payment</c>
/// </summary>
public interface IInvoiceRepository<T, I>
    where T : IInvoice<I>
    where I : notnull
{
    /// <summary>
    /// Gets the invoice associated with the given ID.
    /// </summary>
    /// <param name="id">The ID of the invoice in its own <c>Business</c></param>
    /// <returns>The invoice associated with the given ID.</returns>
    T GetInvoiceById(I id);
}
