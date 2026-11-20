namespace UnfathomablePayLink.Services;

using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Exceptions;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;
using UnfathomablePayLink.Options;

/// <summary>
/// Implements <see cref="IBusinessService"/> handling business CRUD, validation, and orchestration.
/// </summary>
public class BusinessService : IBusinessService
{
    private const string DefaultInvoiceProviderKey = "mock";
    private const string ApiKeyPrefix = "key_";
    private const int ApiKeyTokenLength = 32;
    private const string ApiKeyAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    private readonly IBusinessRepository _businessRepository;
    private readonly Dictionary<string, IInvoiceRepository> _invoiceRepositories;
    private readonly IOptionsMonitor<AdminOptions> _adminOptions;
    private readonly ILogger<BusinessService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessService"/> class.
    /// </summary>
    /// <param name="businessRepository">The repository used to read and persist business entities.</param>
    /// <param name="invoiceRepositories">The registered invoice provider repositories keyed by <see cref="IInvoiceRepository.ProviderKey"/>.</param>
    /// <param name="adminOptions">The administrator options used to prevent generated keys from matching the admin API key.</param>
    /// <param name="logger">The logger used to record business configuration changes and validation failures.</param>
    public BusinessService(
        IBusinessRepository businessRepository,
        IEnumerable<IInvoiceRepository> invoiceRepositories,
        IOptionsMonitor<AdminOptions> adminOptions,
        ILogger<BusinessService> logger)
    {
        _businessRepository = businessRepository;
        _invoiceRepositories = invoiceRepositories.ToDictionary(
            repository => repository.ProviderKey,
            StringComparer.OrdinalIgnoreCase);
        _adminOptions = adminOptions;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<Business>> GetAllBusinessesAsync()
    {
        var businesses = await _businessRepository.GetAllAsync();
        _logger.LogDebug("Retrieved {Count} registered business(es).", businesses.Count);
        return businesses;
    }

    /// <inheritdoc />
    public async Task<Business?> GetBusinessByIdAsync(int id)
    {
        var business = await _businessRepository.GetByIdAsync(id);
        _logger.LogDebug("Looked up business {BusinessId} (Found={Found}).", id, business is not null);
        return business;
    }

    /// <inheritdoc />
    public async Task<Business> CreateBusinessAsync(BusinessCreateDto dto)
    {
        var name = ValidateRequiredField(dto.Name, "Business name cannot be empty.", nameof(Business.Name));
        var document = ValidateRequiredField(dto.Document, "Business document cannot be empty.", nameof(Business.Document));

        var existingByDocument = await _businessRepository.GetByDocumentAsync(document);
        if (existingByDocument is not null)
        {
            _logger.LogWarning(
                "Business creation rejected: document '{Document}' is already registered by business {BusinessId}.",
                document,
                existingByDocument.Id);
            throw new BusinessConflictException($"A business with document '{document}' already exists.");
        }

        var webhookUrl = string.IsNullOrWhiteSpace(dto.WebhookUrl)
            ? null
            : ValidateAndNormalizeWebhookUrl(dto.WebhookUrl);

        var providerKey = ResolveInvoiceProviderKey(
            string.IsNullOrWhiteSpace(dto.InvoiceProviderKey)
                ? DefaultInvoiceProviderKey
                : dto.InvoiceProviderKey);

        var apiKey = await GenerateUniqueApiKeyAsync();

        var business = new Business
        {
            Name = name,
            Document = document,
            ApiKey = apiKey,
            WebhookUrl = webhookUrl,
            InvoiceProviderKey = providerKey
        };

        try
        {
            var created = await _businessRepository.AddAsync(business);
            _logger.LogInformation(
                "Created business {BusinessId} ('{BusinessName}', Document='{Document}', Provider='{ProviderKey}').",
                created.Id,
                created.Name,
                created.Document,
                created.InvoiceProviderKey);
            return created;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Conflict while creating business with document '{Document}'.", document);
            throw new BusinessConflictException($"A business with document '{document}' already exists.");
        }
    }

    /// <inheritdoc />
    public async Task<Business?> UpdateBusinessAsync(int id, BusinessUpdateDto dto)
    {
        var name = ValidateRequiredField(dto.Name, "Business name cannot be empty.", nameof(Business.Name));
        var document = ValidateRequiredField(dto.Document, "Business document cannot be empty.", nameof(Business.Document));

        var existing = await _businessRepository.GetByIdAsync(id);
        if (existing is null)
        {
            _logger.LogWarning("Cannot update business {BusinessId}: not found.", id);
            return null;
        }

        if (!string.Equals(existing.Document, document, StringComparison.Ordinal))
        {
            var existingByDocument = await _businessRepository.GetByDocumentAsync(document);
            if (existingByDocument is not null && existingByDocument.Id != id)
            {
                _logger.LogWarning(
                    "Business update rejected for {BusinessId}: document '{Document}' belongs to business {ConflictBusinessId}.",
                    id,
                    document,
                    existingByDocument.Id);
                throw new BusinessConflictException($"A business with document '{document}' already exists.");
            }
        }

        var webhookUrl = string.IsNullOrWhiteSpace(dto.WebhookUrl)
            ? null
            : ValidateAndNormalizeWebhookUrl(dto.WebhookUrl);

        var providerKey = dto.InvoiceProviderKey is null
            ? existing.InvoiceProviderKey
            : ResolveInvoiceProviderKey(dto.InvoiceProviderKey);

        existing.Name = name;
        existing.Document = document;
        existing.WebhookUrl = webhookUrl;
        existing.InvoiceProviderKey = providerKey;

        try
        {
            await _businessRepository.UpdateAsync(existing);
            _logger.LogInformation(
                "Updated business {BusinessId} ('{BusinessName}', Document='{Document}', Provider='{ProviderKey}').",
                existing.Id,
                existing.Name,
                existing.Document,
                existing.InvoiceProviderKey);
            return existing;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Conflict while updating business {BusinessId} with document '{Document}'.", id, document);
            throw new BusinessConflictException($"A business with document '{document}' already exists.");
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteBusinessAsync(int id)
    {
        var existing = await _businessRepository.GetByIdAsync(id);
        if (existing is null)
        {
            _logger.LogWarning("Cannot delete business {BusinessId}: not found.", id);
            return false;
        }

        if (await _businessRepository.HasPaymentsAsync(id))
        {
            _logger.LogWarning(
                "Business deletion rejected for {BusinessId} ('{BusinessName}'): business has associated payments.",
                id,
                existing.Name);
            throw new BusinessConflictException($"Business with ID {id} cannot be deleted because it has associated payments.");
        }

        try
        {
            var deleted = await _businessRepository.DeleteAsync(id);
            if (deleted)
            {
                _logger.LogInformation(
                    "Deleted business {BusinessId} ('{BusinessName}', Document='{Document}').",
                    existing.Id,
                    existing.Name,
                    existing.Document);
            }

            return deleted;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogWarning(ex, "Deletion blocked for business {BusinessId} due to associated records.", id);
            throw new BusinessConflictException($"Business with ID {id} cannot be deleted because it has associated payments.");
        }
    }

    /// <inheritdoc />
    public async Task<Business?> UpdateWebhookUrlAsync(int id, string webhookUrl)
    {
        string normalizedUrl;
        try
        {
            normalizedUrl = ValidateAndNormalizeWebhookUrl(webhookUrl);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(
                "Invalid webhook URL rejected for business {BusinessId}: {Reason}",
                id,
                ex.Message);
            throw;
        }

        var existing = await _businessRepository.GetByIdAsync(id);
        if (existing == null)
        {
            _logger.LogWarning(
                "Cannot update webhook URL because business {BusinessId} was not found.",
                id);
            return null;
        }

        existing.WebhookUrl = normalizedUrl;
        await _businessRepository.UpdateAsync(existing);

        _logger.LogInformation(
            "Updated webhook URL for business {BusinessId} ('{BusinessName}') to {WebhookUrl}.",
            existing.Id,
            existing.Name,
            normalizedUrl);

        return existing;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveWebhookUrlAsync(int id)
    {
        var existing = await _businessRepository.GetByIdAsync(id);
        if (existing == null)
        {
            _logger.LogWarning(
                "Cannot remove webhook URL because business {BusinessId} was not found.",
                id);
            return false;
        }

        var previousUrl = existing.WebhookUrl;
        existing.WebhookUrl = null;
        await _businessRepository.UpdateAsync(existing);

        _logger.LogInformation(
            "Removed webhook URL for business {BusinessId} ('{BusinessName}', PreviousUrl={PreviousUrl}).",
            existing.Id,
            existing.Name,
            previousUrl);

        return true;
    }

    private string ResolveInvoiceProviderKey(string providerKey)
    {
        var trimmed = providerKey.Trim();
        if (string.IsNullOrEmpty(trimmed) || !_invoiceRepositories.TryGetValue(trimmed, out var repository))
        {
            var available = string.Join(", ", _invoiceRepositories.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase));
            throw new ArgumentException(
                $"Unknown invoice provider key '{providerKey}'. Valid providers: {available}.",
                nameof(Business.InvoiceProviderKey));
        }

        return repository.ProviderKey;
    }

    private async Task<string> GenerateUniqueApiKeyAsync()
    {
        var adminKey = _adminOptions.CurrentValue.ApiKey?.Trim();

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var candidate = $"{ApiKeyPrefix}{RandomNumberGenerator.GetString(ApiKeyAlphabet, ApiKeyTokenLength)}";
            if (!string.IsNullOrEmpty(adminKey) && string.Equals(candidate, adminKey, StringComparison.Ordinal))
            {
                continue;
            }

            if (await _businessRepository.GetFromApiKeyAsync(candidate) is null)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Unable to generate a unique business API key.");
    }

    private static string ValidateRequiredField(string value, string errorMessage, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(errorMessage, paramName);
        }

        return value.Trim();
    }

    private static string ValidateAndNormalizeWebhookUrl(string webhookUrl)
    {
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            throw new ArgumentException("Webhook URL cannot be empty.", nameof(Business.WebhookUrl));
        }

        var trimmed = webhookUrl.Trim();
        if (!Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Webhook URL must be a valid absolute HTTP or HTTPS URL.", nameof(Business.WebhookUrl));
        }

        return trimmed;
    }
}
