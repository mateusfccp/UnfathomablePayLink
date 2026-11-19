namespace UnfathomablePayLink.Interfaces;

using UnfathomablePayLink.Models;

/// <summary>
/// Represents an invoice belonging to a <see cref="Business"/>.
/// </summary>
public interface IInvoice
{
    /// <summary>
    /// Gets the unique identifier of the invoice within its <see cref="Business"/>.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the total monetary amount of the invoice to be paid.
    /// </summary>
    decimal Amount { get; }
}
