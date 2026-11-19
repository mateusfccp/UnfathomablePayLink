namespace UnfathomablePayLink.Repositories;

using Microsoft.EntityFrameworkCore;
using UnfathomablePayLink.Context;
using UnfathomablePayLink.Enums;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Implements <see cref="IPaymentRepository"/> using Entity Framework Core and SQLite.
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly PayLinkContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentRepository"/> class.
    /// </summary>
    /// <param name="context">The database context used to access payment records.</param>
    public PaymentRepository(PayLinkContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Payment?> GetPaymentById(int id)
    {
        return await _context.Payments
            .Include(p => p.Business)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task<List<Payment>> GetPaymentsByInvoiceId(int businessId, string invoiceId)
    {
        return await _context.Payments
            .Include(p => p.Business)
            .Where(p => p.Business.Id == businessId && p.InvoiceId == invoiceId)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Payment>> GetPaymentsByBusinessAsync(int businessId)
    {
        return await _context.Payments
            .Include(p => p.Business)
            .Where(p => p.Business.Id == businessId)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Payment> AddPaymentAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    /// <inheritdoc />
    public async Task<Payment> UpdatePaymentStateAsync(int paymentId, PaymentState state)
    {
        var payment = await _context.Payments
            .Include(p => p.Business)
            .FirstAsync(p => p.Id == paymentId);
        payment.State = state;
        await _context.SaveChangesAsync();
        return payment;
    }
}
