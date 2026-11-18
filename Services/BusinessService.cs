namespace UnfathomablePayLink.Services;

using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Implements <c>IBusinessService</c> handling business validation and orchestration.
/// </summary>
public class BusinessService : IBusinessService
{
    private readonly IBusinessRepository _businessRepository;

    public BusinessService(IBusinessRepository businessRepository)
    {
        _businessRepository = businessRepository;
    }

    public async Task<IEnumerable<Business>> GetAllBusinessesAsync()
    {
        return await _businessRepository.GetAllAsync();
    }

    public async Task<Business?> GetBusinessByIdAsync(int id)
    {
        return await _businessRepository.GetByIdAsync(id);
    }

    public async Task<Business> RegisterBusinessAsync(Business business)
    {
        if (string.IsNullOrWhiteSpace(business.Name))
        {
            throw new ArgumentException("Business name cannot be empty.", nameof(business.Name));
        }

        if (string.IsNullOrWhiteSpace(business.Document))
        {
            throw new ArgumentException("Business document/CUIT cannot be empty.", nameof(business.Document));
        }

        // Validación de unicidad de documento/CUIT
        var existing = await _businessRepository.GetByDocumentAsync(business.Document);
        if (existing != null)
        {
            throw new InvalidOperationException($"A business with document '{business.Document}' already exists.");
        }

        return await _businessRepository.AddAsync(business);
    }

    public async Task<Business?> UpdateBusinessAsync(int id, Business business)
    {
        var existing = await _businessRepository.GetByIdAsync(id);
        if (existing == null)
        {
            return null;
        }

        if (existing.Document != business.Document)
        {
            var documentCheck = await _businessRepository.GetByDocumentAsync(business.Document);
            if (documentCheck != null && documentCheck.Id != id)
            {
                throw new InvalidOperationException($"A business with document '{business.Document}' already exists.");
            }
        }

        existing.Name = business.Name;
        existing.Document = business.Document;

        await _businessRepository.UpdateAsync(existing);
        return existing;
    }

    public async Task<bool> DeleteBusinessAsync(int id)
    {
        return await _businessRepository.DeleteAsync(id);
    }
}
