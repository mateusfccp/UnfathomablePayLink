namespace UnfathomablePayLink.Interfaces;

using UnfathomablePayLink.Models;

/// <summary>
/// Service interface defining business logic operations for businesses.
/// </summary>
public interface IBusinessService
{
    /// <summary>
    /// Retrieves all registered businesses.
    /// </summary>
    Task<IEnumerable<Business>> GetAllBusinessesAsync();

    /// <summary>
    /// Retrieves a single business by its identifier.
    /// </summary>
    /// <param name="id">The business identifier.</param>
    Task<Business?> GetBusinessByIdAsync(int id);

    /// <summary>
    /// Registers a new business applying business rules and validations.
    /// </summary>
    /// <param name="business">The business entity to register.</param>
    Task<Business> RegisterBusinessAsync(Business business);

    /// <summary>
    /// Updates an existing business with new data.
    /// </summary>
    /// <param name="id">The identifier of the business to update.</param>
    /// <param name="business">The updated business information.</param>
    Task<Business?> UpdateBusinessAsync(int id, Business business);

    /// <summary>
    /// Deletes a business by its identifier.
    /// </summary>
    /// <param name="id">The business identifier.</param>
    Task<bool> DeleteBusinessAsync(int id);
}
