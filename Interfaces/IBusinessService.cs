namespace UnfathomablePayLink.Interfaces;

using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Exceptions;
using UnfathomablePayLink.Models;

/// <summary>
/// Defines business logic operations for managing <see cref="Business"/> entities and settings.
/// </summary>
public interface IBusinessService
{
    /// <summary>
    /// Retrieves all registered businesses.
    /// </summary>
    /// <returns>A list of all <see cref="Business"/> records.</returns>
    Task<List<Business>> GetAllBusinessesAsync();

    /// <summary>
    /// Retrieves a single business by its primary key identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the business.</param>
    /// <returns>
    /// The matching <see cref="Business"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<Business?> GetBusinessByIdAsync(int id);

    /// <summary>
    /// Creates a new business with a server-generated API key.
    /// </summary>
    /// <param name="dto">The business creation payload.</param>
    /// <returns>The newly created <see cref="Business"/>.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when required fields, <see cref="Business.WebhookUrl"/>, or <see cref="Business.InvoiceProviderKey"/> are invalid.
    /// </exception>
    /// <exception cref="BusinessConflictException">
    /// Thrown when another business already uses the specified <see cref="Business.Document"/>.
    /// </exception>
    Task<Business> CreateBusinessAsync(BusinessCreateDto dto);

    /// <summary>
    /// Updates an existing business while preserving its server-generated API key.
    /// </summary>
    /// <param name="id">The unique identifier of the business to update.</param>
    /// <param name="dto">The business update payload.</param>
    /// <returns>
    /// The updated <see cref="Business"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when required fields, <see cref="Business.WebhookUrl"/>, or <see cref="Business.InvoiceProviderKey"/> are invalid.
    /// </exception>
    /// <exception cref="BusinessConflictException">
    /// Thrown when another business already uses the specified <see cref="Business.Document"/>.
    /// </exception>
    Task<Business?> UpdateBusinessAsync(int id, BusinessUpdateDto dto);

    /// <summary>
    /// Deletes an existing business if it has no associated payments.
    /// </summary>
    /// <param name="id">The unique identifier of the business to delete.</param>
    /// <returns>
    /// <see langword="true"/> if the business was found and deleted; <see langword="false"/> if not found.
    /// </returns>
    /// <exception cref="BusinessConflictException">
    /// Thrown when the business has associated payments and cannot be deleted.
    /// </exception>
    Task<bool> DeleteBusinessAsync(int id);

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
