namespace UnfathomablePayLink.Services;

using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Implements <see cref="IBusinessService"/> handling business validation and orchestration.
/// </summary>
public class BusinessService : IBusinessService
{
    private readonly IBusinessRepository _businessRepository;
    private readonly ILogger<BusinessService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessService"/> class.
    /// </summary>
    /// <param name="businessRepository">The repository used to read and persist business entities.</param>
    /// <param name="logger">The logger used to record business configuration changes and validation failures.</param>
    public BusinessService(
        IBusinessRepository businessRepository,
        ILogger<BusinessService> logger)
    {
        _businessRepository = businessRepository;
        _logger = logger;
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
