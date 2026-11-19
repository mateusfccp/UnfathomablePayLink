namespace UnfathomablePayLink.Services;

using UnfathomablePayLink.Enums;
using UnfathomablePayLink.Exceptions;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Implements <see cref="IPaymentService"/> containing the core business rules for payment processing.
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IInvoiceService _invoiceService;
    private readonly ILogger<PaymentService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentService"/> class.
    /// </summary>
    /// <param name="paymentRepository">The repository used to read and persist payment records.</param>
    /// <param name="businessRepository">The repository used to resolve business records.</param>
    /// <param name="invoiceService">The service used to retrieve external invoice details.</param>
    /// <param name="logger">The logger used to record payment processing diagnostics.</param>
    /// <param name="scopeFactory">The scope factory used to create DI scopes for asynchronous payment completion.</param>
    public PaymentService(
        IPaymentRepository paymentRepository,
        IBusinessRepository businessRepository,
        IInvoiceService invoiceService,
        ILogger<PaymentService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _paymentRepository = paymentRepository;
        _businessRepository = businessRepository;
        _invoiceService = invoiceService;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    /// <inheritdoc />
    public async Task<List<Payment>> GetPaymentsAsync(int businessId, string? invoiceId = null, PaymentState? state = null)
    {
        var payments = await _paymentRepository.GetPaymentsByBusinessAsync(businessId);

        if (!string.IsNullOrWhiteSpace(invoiceId))
        {
            payments = payments.Where(p => p.InvoiceId == invoiceId).ToList();
        }

        if (state.HasValue)
        {
            payments = payments.Where(p => p.State == state.Value).ToList();
        }

        _logger.LogDebug(
            "Retrieved {Count} payment(s) for business {BusinessId} (InvoiceId={InvoiceId}, State={State}).",
            payments.Count,
            businessId,
            invoiceId,
            state);

        return payments;
    }

    /// <inheritdoc />
    public async Task<Payment?> GetPaymentByIdAsync(int transactionId)
    {
        var payment = await _paymentRepository.GetPaymentById(transactionId);
        _logger.LogDebug(
            "Looked up payment {PaymentId} (Found={Found}).",
            transactionId,
            payment is not null);
        return payment;
    }

    /// <inheritdoc />
    public async Task<Payment> ProcessPaymentAsync(int businessId, string invoiceId)
    {
        if (string.IsNullOrWhiteSpace(invoiceId))
        {
            _logger.LogWarning(
                "Payment creation rejected for business {BusinessId}: invoice ID was empty.",
                businessId);
            throw new ArgumentException("Invoice ID cannot be empty.", nameof(invoiceId));
        }

        var business = await _businessRepository.GetByIdAsync(businessId);
        if (business == null)
        {
            _logger.LogWarning(
                "Payment creation rejected: business {BusinessId} was not found.",
                businessId);
            throw new KeyNotFoundException($"Business with ID {businessId} was not found.");
        }

        var existingPayments = await _paymentRepository.GetPaymentsByInvoiceId(businessId, invoiceId);

        if (existingPayments.Any(p => p.State == PaymentState.Approved))
        {
            _logger.LogWarning(
                "Payment creation rejected for business {BusinessId}, invoice '{InvoiceId}': an approved payment already exists.",
                businessId,
                invoiceId);
            throw new PaymentFinishedException(businessId, invoiceId);
        }

        if (existingPayments.Any(p => p.State == PaymentState.Pending))
        {
            _logger.LogWarning(
                "Payment creation rejected for business {BusinessId}, invoice '{InvoiceId}': a payment is already pending.",
                businessId,
                invoiceId);
            throw new PaymentPendingException(businessId, invoiceId);
        }

        var invoice = await _invoiceService.GetInvoiceByIdAsync(businessId, invoiceId);
        if (invoice == null)
        {
            _logger.LogWarning(
                "Payment creation rejected for business {BusinessId}: invoice '{InvoiceId}' was not found.",
                businessId,
                invoiceId);
            throw new KeyNotFoundException($"Invoice '{invoiceId}' for business {businessId} was not found.");
        }

        var payment = new Payment
        {
            Business = business,
            InvoiceId = invoiceId,
            Amount = invoice.Amount,
            State = PaymentState.Pending,
            PaymentDate = DateTime.UtcNow
        };

        var newPayment = await _paymentRepository.AddPaymentAsync(payment);

        _logger.LogInformation(
            "Created pending payment {PaymentId} for business {BusinessId}, invoice '{InvoiceId}' with amount {Amount}.",
            newPayment.Id,
            businessId,
            invoiceId,
            newPayment.Amount);

        _ = FakePaymentProcess(newPayment.Id);

        return newPayment;
    }

    private async Task FakePaymentProcess(int paymentId)
    {
        try
        {
            var sleepTime = Random.Shared.NextInteger(10000) + 3000;
            _logger.LogDebug(
                "Starting simulated asynchronous processing for payment {PaymentId} (DelayMs={DelayMs}).",
                paymentId,
                sleepTime);

            await Task.Delay(sleepTime);

            var result = Random.Shared.NextInteger(0, 2);
            var state = result == 0 ? PaymentState.Rejected : PaymentState.Approved;

            await using var scope = _scopeFactory.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
            var updatedPayment = await repository.UpdatePaymentStateAsync(paymentId, state);

            _logger.LogInformation(
                "Payment {PaymentId} for business {BusinessId} (invoice '{InvoiceId}') transitioned to {State}.",
                updatedPayment.Id,
                updatedPayment.Business.Id,
                updatedPayment.InvoiceId,
                updatedPayment.State);

            var webhookService = scope.ServiceProvider.GetRequiredService<IWebhookService>();
            await webhookService.NotifyPaymentProcessedAsync(updatedPayment);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to process simulated payment {PaymentId}.", paymentId);
        }
    }
}
