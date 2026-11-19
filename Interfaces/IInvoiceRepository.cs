namespace UnfathomablePayLink.Interfaces;

using UnfathomablePayLink.Models;

/// <summary>
/// Defines the data access contract for retrieving external invoices for a <see cref="Business"/>.
/// </summary>
public interface IInvoiceRepository
{
    /// <summary>
    /// Gets the unique key identifying the billing provider handled by this repository.
    /// </summary>
    string ProviderKey { get; }

    /// <summary>
    /// Asynchronously retrieves the invoice associated with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the invoice within its <see cref="Business"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The matching <see cref="IInvoice"/> if found; otherwise, <see langword="null"/>.</returns>
    Task<IInvoice?> GetInvoiceByIdAsync(string id, CancellationToken cancellationToken = default);
}
