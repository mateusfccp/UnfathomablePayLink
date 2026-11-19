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
    private readonly ILogger<PaymentService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public PaymentService(
        IPaymentRepository paymentRepository,
        IBusinessRepository businessRepository,
        IInvoiceService invoiceService,
    ILogger<PaymentService> logger,
    IServiceScopeFactory scopeFactory
    )
    {
        _paymentRepository = paymentRepository;
        _businessRepository = businessRepository;
        _invoiceService = invoiceService;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task<List<Payment>> GetPaymentsAsync(int businessId, string? invoiceId = null, PaymentState? state = null)
    {
        var payments = await _paymentRepository.GetAllPaymentsAsync();
        payments = payments.Where(p => p.Business.Id == businessId).ToList();

        if (!string.IsNullOrWhiteSpace(invoiceId))
        {
            payments = payments.Where(p => p.InvoiceId == invoiceId).ToList();
        }
        if (state.HasValue)
        {
            payments = payments.Where(p => p.State == state.Value).ToList();
        }

        return payments;
    }


    public async Task<Payment?> GetPaymentByIdAsync(int transactionId)
    {
        return await _paymentRepository.GetPaymentById(transactionId);
    }

    public async Task<Payment> ProcessPaymentAsync(int businessId, string invoiceId)
    {
        if (string.IsNullOrWhiteSpace(invoiceId))
        {
            throw new ArgumentException("Invoice ID cannot be empty.", nameof(invoiceId));
        }

        var business = await _businessRepository.GetByIdAsync(businessId);
        if (business == null)
        {
            throw new KeyNotFoundException($"Business with ID {businessId} was not found.");
        }

        var existingPayments = await _paymentRepository.GetPaymentsByInvoiceId(businessId, invoiceId);

        if (existingPayments.Any(p => p.State == PaymentState.Approved))
        {
            throw new PaymentFinishedException(businessId, invoiceId);
        }

        if (existingPayments.Any(p => p.State == PaymentState.Pending))
        {
            throw new PaymentPendingException(businessId, invoiceId);
        }

        var invoice = await _invoiceService.GetInvoiceByIdAsync(businessId, invoiceId);
        if (invoice == null)
        {
            throw new KeyNotFoundException(); // Fix this
        }

        var paymentAmount = invoice.Amount;

        var payment = new Payment
        {
            Business = business,
            InvoiceId = invoiceId,
            Amount = paymentAmount,
            State = PaymentState.Pending,
            PaymentDate = DateTime.UtcNow
        };

        var newPayment = await _paymentRepository.AddPaymentAsync(payment);

        _ = FakePaymentProcess(newPayment.Id);

        return newPayment;
    }

    private async Task FakePaymentProcess(int paymentId)
    {
        try
        {
            var sleepTime = Random.Shared.NextInteger(10000) + 3000;

            await Task.Delay(sleepTime);

            var result = Random.Shared.NextInteger(0, 2);
            var state = result == 0 ? PaymentState.Rejected : PaymentState.Approved;

            await using var scope = _scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
            var updatedPayment = await repository.UpdatePaymentStateAsync(paymentId, state);

            var webhookService = scope.ServiceProvider.GetRequiredService<IWebhookService>();
            await webhookService.NotifyPaymentProcessedAsync(updatedPayment);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to process simulated payment {PaymentId}.", paymentId);
        }
    }
}
