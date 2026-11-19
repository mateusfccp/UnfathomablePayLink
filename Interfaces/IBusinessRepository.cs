namespace UnfathomablePayLink.Interfaces;

using UnfathomablePayLink.Models;

/// <summary>
/// Defines the data access contract for <see cref="Business"/> entities.
/// </summary>
public interface IBusinessRepository
{
    /// <summary>
    /// Retrieves a business by its primary key identifier.
    /// </summary>
    /// <param name="id">The business identifier.</param>
    /// <returns>
    /// The matching <see cref="Business"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<Business?> GetByIdAsync(int id);

    /// <summary>
    /// Persists updates to an existing <see cref="Business"/> entity.
    /// </summary>
    /// <param name="business">The business entity with updated data.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    Task UpdateAsync(Business business);

    /// <summary>
    /// Retrieves a business by its assigned API key.
    /// </summary>
    /// <param name="apiKey">The API key of the business.</param>
    /// <returns>
    /// The matching <see cref="Business"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<Business?> GetFromApiKeyAsync(string apiKey);
}
