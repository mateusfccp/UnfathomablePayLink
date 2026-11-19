namespace UnfathomablePayLink.Interfaces;

/// <summary>
/// Represents an invoice in a <c>Business</c>.
/// <para>
/// It requires, at a minimum, a string <see cref="Id"/> and an <see cref="Amount"/>.
/// </para>
/// </summary>
public interface IInvoice
{
    /// <summary>
    /// The unique identifier of the invoice within its <c>Business</c>.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// The total monetary amount of the invoice to be paid.
    /// </summary>
    decimal Amount { get; }
}
