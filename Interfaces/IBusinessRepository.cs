namespace UnfathomablePayLink.Interfaces;

using UnfathomablePayLink.Models;

/// <summary>
/// Defines the data access contract for <see cref="Business"/> entities.
/// </summary>
public interface IBusinessRepository
{
    /// <summary>
    /// Retrieves all registered businesses ordered by identifier.
    /// </summary>
    /// <returns>A list of all <see cref="Business"/> records.</returns>
    Task<List<Business>> GetAllAsync();

    /// <summary>
    /// Retrieves a business by its primary key identifier.
    /// </summary>
    /// <param name="id">The business identifier.</param>
    /// <returns>
    /// The matching <see cref="Business"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<Business?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves a business by its tax identification document.
    /// </summary>
    /// <param name="document">The unique business document.</param>
    /// <returns>
    /// The matching <see cref="Business"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<Business?> GetByDocumentAsync(string document);

    /// <summary>
    /// Persists a new <see cref="Business"/> entity to the database.
    /// </summary>
    /// <param name="business">The business entity to add.</param>
    /// <returns>The persisted <see cref="Business"/> entity with its generated identifier.</returns>
    Task<Business> AddAsync(Business business);

    /// <summary>
    /// Persists updates to an existing <see cref="Business"/> entity.
    /// </summary>
    /// <param name="business">The business entity with updated data.</param>
    /// <returns>A task representing the asynchronous update operation.</returns>
    Task UpdateAsync(Business business);

    /// <summary>
    /// Determines whether any payment records are associated with the specified business.
    /// </summary>
    /// <param name="id">The business identifier.</param>
    /// <returns><see langword="true"/> if at least one payment exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> HasPaymentsAsync(int id);

    /// <summary>
    /// Removes a <see cref="Business"/> entity by its identifier.
    /// </summary>
    /// <param name="id">The business identifier.</param>
    /// <returns><see langword="true"/> if the business existed and was deleted; otherwise, <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Retrieves a business by its assigned API key.
    /// </summary>
    /// <param name="apiKey">The API key of the business.</param>
    /// <returns>
    /// The matching <see cref="Business"/> if found; otherwise, <see langword="null"/>.
    /// </returns>
    Task<Business?> GetFromApiKeyAsync(string apiKey);
}
