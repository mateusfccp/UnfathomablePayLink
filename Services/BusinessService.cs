namespace UnfathomablePayLink.Services;

using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Implements <see cref="IBusinessService"/> handling business validation and orchestration.
/// </summary>
public class BusinessService : IBusinessService
{
    private readonly IBusinessRepository _businessRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessService"/> class.
    /// </summary>
    /// <param name="businessRepository">The repository used to read and persist business entities.</param>
    public BusinessService(IBusinessRepository businessRepository)
    {
        _businessRepository = businessRepository;
    }

    /// <inheritdoc />
    public async Task<Business?> UpdateWebhookUrlAsync(int id, string webhookUrl)
    {
        var normalizedUrl = ValidateAndNormalizeWebhookUrl(webhookUrl);

        var existing = await _businessRepository.GetByIdAsync(id);
        if (existing == null)
        {
            return null;
        }

        existing.WebhookUrl = normalizedUrl;
        await _businessRepository.UpdateAsync(existing);
        return existing;
    }

    /// <inheritdoc />
    public async Task<bool> RemoveWebhookUrlAsync(int id)
    {
        var existing = await _businessRepository.GetByIdAsync(id);
        if (existing == null)
        {
            return false;
        }

        existing.WebhookUrl = null;
        await _businessRepository.UpdateAsync(existing);
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
