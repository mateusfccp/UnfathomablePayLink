namespace UnfathomablePayLink.Interfaces;

using UnfathomablePayLink.Models;

/// <summary>
/// Interface <c>IBusinessRepository</c> defines the data access contract for
/// business entities.
/// </summary>
public interface IBusinessRepository
{
    /// <summary>
    /// Retrieves all registered businesses.
    /// </summary>
    Task<IEnumerable<Business>> GetAllAsync();

    /// <summary>
    /// Retrieves a business by its identifier.
    /// </summary>
    /// <param name="id">The business identifier.</param>
    Task<Business?> GetByIdAsync(int id);

    /// <summary>
    /// Retrieves a business by its identification document / CUIT.
    /// </summary>
    /// <param name="document">The document string.</param>
    Task<Business?> GetByDocumentAsync(string document);

    /// <summary>
    /// Adds a new business entity to the database.
    /// </summary>
    /// <param name="business">The business to add.</param>
    Task<Business> AddAsync(Business business);

    /// <summary>
    /// Updates an existing business entity in the database.
    /// </summary>
    /// <param name="business">The business entity with updated data.</param>
    Task UpdateAsync(Business business);

    /// <summary>
    /// Removes a business entity from the database.
    /// </summary>
    /// <param name="id">The identifier of the business to delete.</param>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Checks whether a business with the specified ID exists.
    /// </summary>
    /// <param name="id">The business identifier.</param>
    Task<bool> ExistsAsync(int id);

    /// <summary>
    /// Gets the business based on its associated API key.
    /// </summary>
    /// <param name="apiKey">The API Key of the business.</param>
    Task<Business?> GetFromApiKeyAsync(String apiKey);
}
