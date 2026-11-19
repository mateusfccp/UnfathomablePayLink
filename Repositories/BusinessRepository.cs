namespace UnfathomablePayLink.Repositories;

using Microsoft.EntityFrameworkCore;
using UnfathomablePayLink.Context;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Implements <c>IBusinessRepository</c> using Entity Framework Core and SQLite.
/// </summary>
public class BusinessRepository : IBusinessRepository
{
    private readonly PayLinkContext _context;

    public BusinessRepository(PayLinkContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Business>> GetAllAsync()
    {
        return await _context.Businesses.AsNoTracking().ToListAsync();
    }

    public async Task<Business?> GetByIdAsync(int id)
    {
        return await _context.Businesses.FindAsync(id);
    }

    public async Task<Business?> GetByDocumentAsync(string document)
    {
        return await _context.Businesses
            .FirstOrDefaultAsync(b => b.Document == document);
    }

    public async Task<Business> AddAsync(Business business)
    {
        await _context.Businesses.AddAsync(business);
        await _context.SaveChangesAsync();
        return business;
    }

    public async Task UpdateAsync(Business business)
    {
        _context.Businesses.Update(business);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var business = await _context.Businesses.FindAsync(id);
        if (business == null)
        {
            return false;
        }

        _context.Businesses.Remove(business);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Businesses.AnyAsync(b => b.Id == id);
    }

    public async Task<Business?> GetFromApiKeyAsync(string apiKey)
    {
        return await _context.Businesses
            .SingleOrDefaultAsync(b => b.ApiKey == apiKey);
    }
}
