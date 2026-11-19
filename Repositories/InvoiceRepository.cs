namespace UnfathomablePayLink.Repositories;

using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Mock implementation of <see cref="IInvoiceRepository"/> simulating invoice retrieval from an external business billing API.
/// </summary>
public class InvoiceRepository : IInvoiceRepository
{
    private readonly Dictionary<string, Invoice> _invoices = new()
    {
        {
            "INV-001",
            new Invoice
            {
                Id = "INV-001",
                Amount = 1500.50m,
                Description = "Servicio de Internet Mensual",
                DueDate = DateTime.UtcNow.AddDays(15),
                IsPaid = false
            }
        },
        {
            "INV-002",
            new Invoice
            {
                Id = "INV-002",
                Amount = 3200.00m,
                Description = "Suscripción Software",
                DueDate = DateTime.UtcNow.AddDays(7),
                IsPaid = false
            }
        }
    };

    /// <inheritdoc />
    public string ProviderKey => "mock";

    /// <inheritdoc />
    public Task<IInvoice?> GetInvoiceByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_invoices.TryGetValue(id, out var invoice))
        {
            return Task.FromResult<IInvoice?>(invoice);
        }

        IInvoice simulated = new Invoice
        {
            Id = id,
            Amount = 1000.00m,
            Description = $"Factura simulada {id}",
            DueDate = DateTime.UtcNow.AddDays(30),
            IsPaid = false
        };

        return Task.FromResult<IInvoice?>(simulated);
    }
}
