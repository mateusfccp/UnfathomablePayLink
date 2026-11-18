namespace UnfathomablePayLink.Services;

using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Implements <c>IInvoiceService</c> handling logic for external invoice retrieval and validation.
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IInvoiceRepository<Invoice, string> _invoiceRepository;

    public InvoiceService(
        IBusinessRepository businessRepository,
        IInvoiceRepository<Invoice, string> invoiceRepository)
    {
        _businessRepository = businessRepository;
        _invoiceRepository = invoiceRepository;
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(int businessId, string invoiceId)
    {
        if (string.IsNullOrWhiteSpace(invoiceId))
        {
            throw new ArgumentException("Invoice ID cannot be empty.", nameof(invoiceId));
        }

        var business = await _businessRepository.GetByIdAsync(businessId);
        if (business == null)
        {
            throw new KeyNotFoundException($"Business with ID {businessId} was not found.");
        }

        var invoice = _invoiceRepository.GetInvoiceById(invoiceId);
        return await Task.FromResult(invoice);
    }

    public async Task<bool> ValidateInvoiceAmountAsync(int businessId, string invoiceId, decimal amount)
    {
        var invoice = await GetInvoiceByIdAsync(businessId, invoiceId);
        if (invoice == null)
        {
            return false;
        }

        return invoice.Amount == amount;
    }
}
