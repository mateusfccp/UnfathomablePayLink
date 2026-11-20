namespace UnfathomablePayLink.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UnfathomablePayLink.Authentication;
using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Enums;
using UnfathomablePayLink.Exceptions;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Exposes endpoints for querying and creating payment transactions for the authenticated <see cref="Business"/>.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationPolicies.BusinessOnly)]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly Converter<Payment, PaymentDto> _paymentConverter;
    private readonly ILogger<PaymentsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PaymentsController"/> class.
    /// </summary>
    /// <param name="paymentService">The service used to query and process payments.</param>
    /// <param name="paymentConverter">The converter used to map <see cref="Payment"/> entities to <see cref="PaymentDto"/> instances.</param>
    /// <param name="logger">The logger used to record payment endpoint access and authorization events.</param>
    public PaymentsController(
        IPaymentService paymentService,
        Converter<Payment, PaymentDto> paymentConverter,
        ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _paymentConverter = paymentConverter;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves recorded payment transactions for the authenticated business, optionally filtered by invoice or state.
    /// </summary>
    /// <param name="invoiceId">Optional invoice identifier to filter payments by.</param>
    /// <param name="state">Optional <see cref="PaymentState"/> to filter payments by.</param>
    /// <returns>An <see cref="ActionResult{TValue}"/> containing the matching <see cref="PaymentDto"/> collection.</returns>
    /// <response code="200">Returns the list of payments belonging to the authenticated business.</response>
    /// <response code="401">Missing or invalid API Key.</response>
    [HttpGet]
    [ProducesResponseType<IEnumerable<PaymentDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll(
        [FromQuery] string? invoiceId = null,
        [FromQuery] PaymentState? state = null)
    {
        var business = HttpContext.Features.Get<Business>()!;

        var payments = await _paymentService.GetPaymentsAsync(business.Id, invoiceId, state);
        return Ok(payments.ConvertAll(_paymentConverter));
    }

    /// <summary>
    /// Retrieves a specific payment transaction by its primary key identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the payment.</param>
    /// <returns>An <see cref="ActionResult{TValue}"/> containing the requested <see cref="PaymentDto"/>.</returns>
    /// <response code="200">Returns the requested payment.</response>
    /// <response code="401">Missing or invalid API Key.</response>
    /// <response code="403">The payment belongs to a different business.</response>
    /// <response code="404">Payment not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<PaymentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> GetById(int id)
    {
        var business = HttpContext.Features.Get<Business>()!;

        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            _logger.LogDebug(
                "Payment {PaymentId} requested by business {BusinessId} was not found.",
                id,
                business.Id);
            return NotFound(new { message = $"Payment with ID {id} was not found." });
        }

        if (payment.Business.Id != business.Id)
        {
            _logger.LogWarning(
                "Business {BusinessId} attempted unauthorized access to payment {PaymentId} owned by business {OwnerBusinessId}.",
                business.Id,
                id,
                payment.Business.Id);
            return Forbid();
        }

        return Ok(_paymentConverter(payment));
    }

    /// <summary>
    /// Registers a new payment transaction for the specified invoice and initiates asynchronous processing.
    /// </summary>
    /// <param name="dto">The payment creation payload containing the target invoice identifier.</param>
    /// <returns>An <see cref="ActionResult{TValue}"/> containing the newly created <see cref="PaymentDto"/>.</returns>
    /// <response code="201">Payment created successfully.</response>
    /// <response code="400">Invalid or missing invoice ID.</response>
    /// <response code="401">Missing or invalid API Key.</response>
    /// <response code="404">Business or invoice not found.</response>
    /// <response code="409">A pending or approved payment already exists for this invoice.</response>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType<PaymentDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentDto>> Create([FromBody] PaymentCreateDto dto)
    {
        var business = HttpContext.Features.Get<Business>()!;

        if (string.IsNullOrWhiteSpace(dto.InvoiceId))
        {
            _logger.LogWarning(
                "Rejected payment creation request from business {BusinessId}: missing invoiceId.",
                business.Id);
            return BadRequest(new { message = "El código de factura (invoiceId) es obligatorio." });
        }

        try
        {
            var payment = await _paymentService.ProcessPaymentAsync(business.Id, dto.InvoiceId);
            var resultDto = _paymentConverter(payment);
            return CreatedAtAction(nameof(GetById), new { id = resultDto.Id }, resultDto);
        }
        catch (PaymentPendingException ex)
        {
            return Conflict(new
            {
                message = "A payment is already pending for this invoice.",
                ex.businessId,
                ex.invoiceId
            });
        }
        catch (PaymentFinishedException ex)
        {
            return Conflict(new
            {
                message = "A payment has already been completed for this invoice.",
                ex.businessId,
                ex.invoiceId
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
