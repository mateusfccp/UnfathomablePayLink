/// <summary>
/// An invoice in a <c>Business</c>.
///
/// It requires, at a minimum, an <c>Id</c>, which may have any type <c>I</c>,
/// and an <c>Amount</c>.
/// </summary>
/// <typeparam name="I">The type of the ID in the invoice.</typeparam>
public interface IInvoice<I> where I : notnull
{
    /// <summary>
    /// The unique identifier of the invoice within its <c>Business</c>.
    /// </summary>
    I Id { get; }

    /// <summary>
    /// The total value of the invoice to me paid.
    /// </summary>
    decimal Amount { get; }
}
