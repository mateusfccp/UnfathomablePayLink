namespace UnfathomablePayLink.Interfaces;

/// <summary>
/// Interface <c>IInvoiceRepository</c> defines the interface of an invoice
/// repository.
///
/// The interface describes the repository that gets the invoices of a
/// <c>Business</c> and its details, so that they may be used to generate a
/// <c>Payment</c>
/// </summary>
public interface IInvoiceRepository<T, I> where T : IInvoice<I>
{
    /// <summary>
    /// Gets an invoice from the provided ID.
    /// </summary>
    /// <param name="id">The ID of the invoice in its own <c>Business</c></param>
    T GetInvoiceById(I id);
}
