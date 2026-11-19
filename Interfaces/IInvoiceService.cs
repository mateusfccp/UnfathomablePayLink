namespace UnfathomablePayLink.Interfaces;

/// <summary>
/// Service interface defining operations for querying and validating
/// external invoices from associated businesses.
/// </summary>
public interface IInvoiceService
{
    /// <summary>
    /// Queries the external invoice details from the specified business.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="invoiceId">The raw invoice identifier received by PayLink for the business.</param>
    /// <returns>The matching <see cref="IInvoice"/> details if found; otherwise, <see langword="null"/>.</returns>
    Task<IInvoice?> GetInvoiceByIdAsync(int businessId, string invoiceId);
}
