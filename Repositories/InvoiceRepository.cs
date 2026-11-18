namespace UnfathomablePayLink.Repositories;

using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Repository implementation for retrieving invoices from external businesses.
/// Simulates communication with external business billing APIs (GET /bills/{bill_id})
/// as required by the assignment specification.
/// </summary>
public class InvoiceRepository : IInvoiceRepository<Invoice, string>
{
    private readonly MockInvoiceRepository _mock = new();

    public Invoice GetInvoiceById(string id)
    {
        return _mock.GetInvoiceById(id);
    }
}
