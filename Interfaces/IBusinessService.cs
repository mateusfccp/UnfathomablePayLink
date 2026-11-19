namespace UnfathomablePayLink.Interfaces;

using UnfathomablePayLink.Models;

/// <summary>
/// Defines business logic operations for managing <see cref="Business"/> settings.
/// </summary>
public interface IBusinessService
{
    /// <summary>
    /// Updates the webhook URL configured for the specified business.
    /// </summary>
    /// <param name="id">The unique identifier of the business.</param>
    /// <param name="webhookUrl">The new absolute HTTP or HTTPS webhook URL.</param>
    /// <returns>
    /// The updated <see cref="Business"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="webhookUrl"/> is empty, whitespace, or not a valid absolute HTTP or HTTPS URL.
    /// </exception>
    Task<Business?> UpdateWebhookUrlAsync(int id, string webhookUrl);

    /// <summary>
    /// Clears the webhook URL configured for the specified business.
    /// </summary>
    /// <param name="id">The unique identifier of the business.</param>
    /// <returns>
    /// <see langword="true"/> if the business was found and updated; otherwise, <see langword="false"/>.
    /// </returns>
    Task<bool> RemoveWebhookUrlAsync(int id);
}
