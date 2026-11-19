namespace UnfathomablePayLink.Repositories;

using Microsoft.EntityFrameworkCore;
using UnfathomablePayLink.Context;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Implements <see cref="IBusinessRepository"/> using Entity Framework Core and SQLite.
/// </summary>
public class BusinessRepository : IBusinessRepository
{
    private readonly PayLinkContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRepository"/> class.
    /// </summary>
    /// <param name="context">The database context used to access business records.</param>
    public BusinessRepository(PayLinkContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Business?> GetByIdAsync(int id)
    {
        return await _context.Businesses.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Business business)
    {
        _context.Businesses.Update(business);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<Business?> GetFromApiKeyAsync(string apiKey)
    {
        return await _context.Businesses
            .SingleOrDefaultAsync(b => b.ApiKey == apiKey);
    }
}
