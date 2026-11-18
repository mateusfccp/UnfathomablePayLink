namespace UnfathomablePayLink.Services;

using UnfathomablePayLink.Enums;
using UnfathomablePayLink.Exceptions;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Implements <c>IPaymentService</c> containing the core business rules for payment processing.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IInvoiceService _invoiceService;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IBusinessRepository businessRepository,
        IInvoiceService invoiceService)
    {
        _paymentRepository = paymentRepository;
        _businessRepository = businessRepository;
        _invoiceService = invoiceService;
    }

    public async Task<Payment?> GetPaymentByIdAsync(int transactionId)
    {
        return await _paymentRepository.GetPaymentById(transactionId);
    }

    public async Task<List<Payment>> GetPaymentsByInvoiceAsync(int businessId, string invoiceId)
    {
        return await _paymentRepository.GetPaymentsByInvoiceId(businessId, invoiceId);
    }

    public async Task<Payment?> GetLatestPaymentByInvoiceAsync(int businessId, string invoiceId)
    {
        return await _paymentRepository.GetLatestPaymentByInvoiceId(businessId, invoiceId);
    }

    public async Task<Payment?> GetPaymentByInvoiceIdAsync(string invoiceId)
    {
        // Si el repositorio tiene el método específico, lo usamos
        if (_paymentRepository is Repositories.PaymentRepository repo)
        {
            return await repo.GetLatestPaymentByInvoiceIdOnly(invoiceId);
        }

        return null;
    }

    public async Task<Payment> ProcessPaymentAsync(int businessId, string invoiceId, decimal? amount = null, PaymentState? state = null)
    {
        if (string.IsNullOrWhiteSpace(invoiceId))
        {
            throw new ArgumentException("Invoice ID cannot be empty.", nameof(invoiceId));
        }

        if (amount.HasValue && amount.Value < 0)
        {
            throw new ArgumentException("Payment amount cannot be negative.", nameof(amount));
        }

        // 1. Validar que el negocio exista en el sistema
        var business = await _businessRepository.GetByIdAsync(businessId);
        if (business == null)
        {
            throw new KeyNotFoundException($"Business with ID {businessId} was not found.");
        }

        // 2. Verificar pagos previos para la factura indicada
        var existingPayments = await _paymentRepository.GetPaymentsByInvoiceId(businessId, invoiceId);

        if (existingPayments.Any(p => p.state == PaymentState.Approved))
        {
            throw new PaymentFinishedException(businessId, invoiceId);
        }

        if (existingPayments.Any(p => p.state == PaymentState.Pending))
        {
            throw new PaymentPendingException(businessId, invoiceId);
        }

        // 3. Determinar o validar el monto con la factura externa si no se especificó
        decimal paymentAmount = amount ?? 0m;
        if (paymentAmount <= 0)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(businessId, invoiceId);
            if (invoice != null)
            {
                paymentAmount = invoice.Amount;
            }
        }

        // 4. Crear y registrar la transacción de pago
        var payment = new Payment
        {
            Id = 0,
            Business = business,
            InvoiceId = invoiceId,
            amount = paymentAmount,
            state = state ?? PaymentState.Approved,
            PaymentDate = DateTime.UtcNow
        };

        return await _paymentRepository.AddPaymentAsync(payment);
    }

    public async Task<Invoice> ConsultInvoiceAsync(int businessId, string invoiceId)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(businessId, invoiceId);
        if (invoice == null)
        {
            throw new KeyNotFoundException($"Invoice {invoiceId} was not found for Business {businessId}.");
        }

        return invoice;
    }
}
