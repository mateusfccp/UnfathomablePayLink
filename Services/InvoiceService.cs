namespace UnfathomablePayLink.Services;

using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Implements <c>IInvoiceService</c> handling logic for external invoice retrieval and validation.
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly IBusinessRepository _businessRepository;
    private readonly Dictionary<string, IInvoiceRepository> _repositories;

    public InvoiceService(
        IBusinessRepository businessRepository,
	IEnumerable<IInvoiceRepository> invoiceRepositories)
    {
        _businessRepository = businessRepository;
        _repositories = invoiceRepositories.ToDictionary(
	    repository => repository.ProviderKey,
	    StringComparer.OrdinalIgnoreCase
	);
    }

    public async Task<IInvoice?> GetInvoiceByIdAsync(int businessId, string invoiceId)
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

        var repository = ResolveRepository(business);
        return await repository.GetInvoiceByIdAsync(invoiceId);
    }

    private IInvoiceRepository ResolveRepository(Business business)
    {
        var providerKey = "mock";
        if (!_repositories.TryGetValue(providerKey, out var repository))
        {
            throw new InvalidOperationException(
                $"No invoice repository registered for provider '{providerKey}'.");
        }
        return repository;
    }
}
