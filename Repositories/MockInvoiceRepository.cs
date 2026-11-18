namespace UnfathomablePayLink.Repositories;

using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Mock implementation of <c>IInvoiceRepository</c> for simulating external
/// business billing APIs as requested in the specification.
/// </summary>
public class MockInvoiceRepository : IInvoiceRepository<Invoice, string>
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

    public Invoice GetInvoiceById(string id)
    {
        if (_invoices.TryGetValue(id, out var invoice))
        {
            return invoice;
        }

        // Si no está en el diccionario precargado, generamos una simulación válida
        return new Invoice
        {
            Id = id,
            Amount = 1000.00m,
            Description = $"Factura simulada {id}",
            DueDate = DateTime.UtcNow.AddDays(30),
            IsPaid = false
        };
    }
}
