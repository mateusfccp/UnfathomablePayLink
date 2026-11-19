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
    private readonly ILogger<InvoiceService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="InvoiceService"/> class.
    /// </summary>
    /// <param name="businessRepository">The repository used to validate and retrieve business records.</param>
    /// <param name="invoiceRepositories">The registered invoice provider repositories keyed by <see cref="IInvoiceRepository.ProviderKey"/>.</param>
    /// <param name="logger">The logger used to record external invoice lookup diagnostics.</param>
    public InvoiceService(
        IBusinessRepository businessRepository,
        IEnumerable<IInvoiceRepository> invoiceRepositories,
        ILogger<InvoiceService> logger)
    {
        _businessRepository = businessRepository;
        _repositories = invoiceRepositories.ToDictionary(
            repository => repository.ProviderKey,
            StringComparer.OrdinalIgnoreCase);
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IInvoice?> GetInvoiceByIdAsync(int businessId, string invoiceId)
    {
        if (string.IsNullOrWhiteSpace(invoiceId))
        {
            _logger.LogWarning(
                "Invoice lookup rejected for business {BusinessId}: invoice ID was empty.",
                businessId);
            throw new ArgumentException("Invoice ID cannot be empty.", nameof(invoiceId));
        }

        var business = await _businessRepository.GetByIdAsync(businessId);
        if (business == null)
        {
            _logger.LogWarning(
                "Invoice lookup failed: business {BusinessId} was not found.",
                businessId);
            throw new KeyNotFoundException($"Business with ID {businessId} was not found.");
        }

        var repository = ResolveRepository(business);
        _logger.LogDebug(
            "Querying external invoice '{InvoiceId}' for business {BusinessId} via provider '{ProviderKey}'.",
            invoiceId,
            businessId,
            repository.ProviderKey);

        var invoice = await repository.GetInvoiceByIdAsync(invoiceId);
        if (invoice is null)
        {
            _logger.LogWarning(
                "External invoice '{InvoiceId}' was not found for business {BusinessId} via provider '{ProviderKey}'.",
                invoiceId,
                businessId,
                repository.ProviderKey);
        }
        else
        {
            _logger.LogDebug(
                "Resolved external invoice '{InvoiceId}' for business {BusinessId} (Amount={Amount}).",
                invoice.Id,
                businessId,
                invoice.Amount);
        }

        return invoice;
    }

    private IInvoiceRepository ResolveRepository(Business business)
    {
        var providerKey = "mock";
        if (!_repositories.TryGetValue(providerKey, out var repository))
        {
            _logger.LogError(
                "No invoice repository registered for provider '{ProviderKey}' while resolving business {BusinessId}.",
                providerKey,
                business.Id);
            throw new InvalidOperationException(
                $"No invoice repository registered for provider '{providerKey}'.");
        }

        return repository;
    }
}
