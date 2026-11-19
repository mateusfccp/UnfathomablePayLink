namespace UnfathomablePayLink.Repositories;

using Microsoft.EntityFrameworkCore;
using UnfathomablePayLink.Context;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Implements <c>IPaymentRepository</c> using Entity Framework Core and SQLite.
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly PayLinkContext _context;

    public PaymentRepository(PayLinkContext context)
    {
        _context = context;
    }

    public async Task<Payment?> GetPaymentById(int id)
    {
        return await _context.Payments
            .Include(p => p.Business)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Payment>> GetPaymentsByInvoiceId(int businessId, string invoiceId)
    {
        return await _context.Payments
            .Include(p => p.Business)
            .Where(p => p.Business.Id == businessId && p.InvoiceId == invoiceId)
            .ToListAsync();
    }

    public async Task<Payment?> GetLatestPaymentByInvoiceId(int businessId, string invoiceId)
    {
        return await _context.Payments
            .Include(p => p.Business)
            .Where(p => p.Business.Id == businessId && p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Payment>> GetAllPaymentsAsync()
    {
        return await _context.Payments
            .Include(p => p.Business)
            .ToListAsync();
    }

    public async Task<List<Payment>> GetPaymentsByBusinessAsync(int businessId)
    {
        return await _context.Payments
            .Include(p => p.Business)
            .Where(p => p.Business.Id == businessId)
            .ToListAsync();
    }

    public async Task<Payment> AddPaymentAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
        return payment;
    }
}
