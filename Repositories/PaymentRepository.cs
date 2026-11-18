namespace UnfathomablePayLink.Repositories;

using Microsoft.EntityFrameworkCore;
using UnfathomablePayLink.Context;
using UnfathomablePayLink.Enums;
using UnfathomablePayLink.Exceptions;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Implements <c>IPaymentRepository</c> using Entity Framework Core and SQLite.
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly PayLinkContext _context;
    private readonly IInvoiceRepository<Invoice, string>? _invoiceRepository;

    public PaymentRepository(
        PayLinkContext context,
        IInvoiceRepository<Invoice, string>? invoiceRepository = null)
    {
        _context = context;
        _invoiceRepository = invoiceRepository;
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

    /// <summary>
    /// Gets the latest payment associated with an invoice code regardless of business ID.
    /// </summary>
    public async Task<Payment?> GetPaymentByInvoiceId(string invoiceId)
    {
        return await _context.Payments
            .Include(p => p.Business)
            .Where(p => p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.Id)
            .FirstOrDefaultAsync();
    }

    public async Task CreatePaymentForInvoiceId(int business, string invoiceId)
    {
        var businessEntity = await _context.Businesses.FindAsync(business);
        if (businessEntity == null)
        {
            throw new KeyNotFoundException($"Business with ID {business} was not found.");
        }

        var existingPayments = await GetPaymentsByInvoiceId(business, invoiceId);

        if (existingPayments.Any(p => p.state == PaymentState.Approved))
        {
            throw new PaymentFinishedException(business, invoiceId);
        }

        if (existingPayments.Any(p => p.state == PaymentState.Pending))
        {
            throw new PaymentPendingException(business, invoiceId);
        }

        decimal amount = 0m;
        if (_invoiceRepository != null)
        {
            try
            {
                var invoice = _invoiceRepository.GetInvoiceById(invoiceId);
                if (invoice != null)
                {
                    amount = invoice.Amount;
                }
            }
            catch
            {
                // Fallback to default amount if invoice query fails
            }
        }

        var payment = new Payment
        {
            Id = 0,
            Business = businessEntity,
            InvoiceId = invoiceId,
            amount = amount,
            state = PaymentState.Approved,
            PaymentDate = DateTime.UtcNow
        };

        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
    }

    public async Task<Payment> AddPaymentAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
        await _context.SaveChangesAsync();
        return payment;
    }
}
