namespace UnfathomablePayLink.Interfaces;

/// <summary>
/// Interface <c>IInvoiceRepository</c> defines the interface of an invoice
/// repository.
/// <para>
/// The interface describes the repository that gets the invoices of a
/// <c>Business</c> and its details, so that they may be used to generate a
/// <c>Payment</c>.
/// </para>
/// </summary>
public interface IInvoiceRepository
{
    /// <summary>
    /// Gets the unique key identifying the business integration or billing provider handled by this repository.
    /// </summary>
    string ProviderKey { get; }

    /// <summary>
    /// Asynchronously retrieves the invoice associated with the given identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice within its <c>Business</c>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching <see cref="IInvoice"/> if found; otherwise, <see langword="null"/>.</returns>
    Task<IInvoice?> GetInvoiceByIdAsync(string id, CancellationToken cancellationToken = default);
}
