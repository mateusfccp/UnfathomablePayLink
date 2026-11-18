namespace UnfathomablePayLink.Interfaces;

using UnfathomablePayLink.Models;

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
    /// <param name="invoiceId">The invoice code/number.</param>
    /// <returns>The invoice details, or null if not found.</returns>
    Task<Invoice?> GetInvoiceByIdAsync(int businessId, string invoiceId);

    /// <summary>
    /// Validates whether the invoice exists and its amount matches the payment amount.
    /// </summary>
    /// <param name="businessId">The business ID.</param>
    /// <param name="invoiceId">The invoice code/number.</param>
    /// <param name="amount">The amount to validate.</param>
    /// <returns>True if the invoice is valid for the given amount; otherwise false.</returns>
    Task<bool> ValidateInvoiceAmountAsync(int businessId, string invoiceId, decimal amount);
}
