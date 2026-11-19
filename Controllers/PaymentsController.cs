namespace UnfathomablePayLink.Controllers;

using Microsoft.AspNetCore.Mvc;
using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Enums;
using UnfathomablePayLink.Exceptions;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly Converter<Payment, PaymentDto> _paymentConverter;

    public PaymentsController(
        IPaymentService paymentService,
        Converter<Payment, PaymentDto> paymentConverter)
    {
        _paymentService = paymentService;
        _paymentConverter = paymentConverter;
    }

    /// <summary>
    /// GET api/payments
    /// Retrieves recorded payment transactions filtered by query parameters, mapped to DTOs.
    /// </summary>
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
    /// GET api/payments/{id}
    /// Retrieves a specific payment by its primary key ID mapped to a DTO.
    /// </summary>
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
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            return NotFound(new { message = $"Payment with ID {id} was not found." });
        }

        var business = HttpContext.Features.Get<Business>()!;

        if (payment.Business.Id != business.Id)
	{
            return Forbid();
        }

        return Ok(_paymentConverter(payment));
    }

   
    /// <summary>
    /// POST api/payments
    /// Registers a new payment transaction from a JSON body.
    /// (Requirement: POST /payments)
    /// </summary>
    /// <response code="201">Payment created successfully.</response>
    /// <response code="400">Invalid request payload or negative amount.</response>
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

        if (string.IsNullOrWhiteSpace(dto.InvoiceId))
        {
            return BadRequest(new { message = "El código de factura (invoiceId) es obligatorio." });
        }

        var business = HttpContext.Features.Get<Business>()!;

        return await ProcessCreate(business.Id, dto.InvoiceId, dto);
    }

    private async Task<ActionResult<PaymentDto>> ProcessCreate(int businessId, string invoiceId, PaymentCreateDto? dto)
    {
        try
        {
            var payment = await _paymentService.ProcessPaymentAsync(businessId, invoiceId, dto?.Amount);
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
