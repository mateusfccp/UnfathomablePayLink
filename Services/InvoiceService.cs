namespace UnfathomablePayLink.Services;

using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Implements <see cref="IInvoiceService"/> handling external invoice retrieval and provider resolution.
/// </summary>
public class InvoiceService : IInvoiceService
{
    private readonly IBusinessRepository _businessRepository;
    private readonly Dictionary<string, IInvoiceRepository> _repositories;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceService"/> class.
    /// </summary>
    /// <param name="businessRepository">The repository used to validate and retrieve business records.</param>
    /// <param name="invoiceRepositories">The registered invoice provider repositories keyed by <see cref="IInvoiceRepository.ProviderKey"/>.</param>
    public InvoiceService(
        IBusinessRepository businessRepository,
        IEnumerable<IInvoiceRepository> invoiceRepositories)
    {
        _businessRepository = businessRepository;
        _repositories = invoiceRepositories.ToDictionary(
            repository => repository.ProviderKey,
            StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
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
