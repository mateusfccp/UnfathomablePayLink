namespace UnfathomablePayLink.Controllers;

using Microsoft.AspNetCore.Mvc;
using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Exceptions;
using UnfathomablePayLink.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// GET api/payments/{id}
    /// Retrieves a specific payment by its primary key ID mapped to a DTO.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaymentDto>> GetById(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            return NotFound(new { message = $"Payment with ID {id} was not found." });
        }

        return Ok(payment.ToDto());
    }

    /// <summary>
    /// GET api/payments/business/{businessId}/invoice/{invoiceId}
    /// Retrieves all payment attempts for a given business invoice mapped to DTOs.
    /// </summary>
    [HttpGet("business/{businessId:int}/invoice/{invoiceId}")]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByInvoice(int businessId, string invoiceId)
    {
        var payments = await _paymentService.GetPaymentsByInvoiceAsync(businessId, invoiceId);
        return Ok(payments.Select(p => p.ToDto()));
    }

    /// <summary>
    /// GET api/payments/business/{businessId}/invoice/{invoiceId}/latest
    /// Retrieves the most recent payment attempt for a given business invoice mapped to a DTO.
    /// </summary>
    [HttpGet("business/{businessId:int}/invoice/{invoiceId}/latest")]
    public async Task<ActionResult<PaymentDto>> GetLatestPaymentByInvoice(int businessId, string invoiceId)
    {
        var payment = await _paymentService.GetLatestPaymentByInvoiceAsync(businessId, invoiceId);
        if (payment == null)
        {
            return NotFound(new { message = $"No payment records found for Business {businessId} and Invoice {invoiceId}." });
        }

        return Ok(payment.ToDto());
    }

    /// <summary>
    /// POST api/payments/business/{businessId}/invoice/{invoiceId}
    /// Attempts to process and create a new payment for a business invoice using an optional input DTO.
    /// </summary>
    [HttpPost("business/{businessId:int}/invoice/{invoiceId}")]
    public async Task<ActionResult<PaymentDto>> CreatePayment(int businessId, string invoiceId, [FromBody] PaymentCreateDto? dto = null)
    {
        try
        {
            var payment = await _paymentService.ProcessPaymentAsync(businessId, invoiceId, dto?.Amount, dto?.State);
            var resultDto = payment.ToDto();
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
