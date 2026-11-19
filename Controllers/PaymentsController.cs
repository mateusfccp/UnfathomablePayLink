namespace UnfathomablePayLink.Controllers;

using Microsoft.AspNetCore.Mvc;
using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Enums;
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
    /// GET api/payments?businessId=1&invoiceId=INV-001&state=Approved
    /// Retrieves recorded payment transactions filtered by query parameters, mapped to DTOs.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll(
        [FromQuery] string? invoiceId = null,
        [FromQuery] PaymentState? state = null)
    {
        var payments = await _paymentService.GetPaymentsAsync(0, invoiceId, state);
        return Ok(payments.Select(p => p.ToDto()));
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
    /// POST api/payments
    /// Registers a new payment transaction from a JSON body.
    /// (Requirement: POST /payments)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PaymentDto>> Create([FromBody] PaymentCreateDto dto)
    {
        if (!dto.BusinessId.HasValue)
        {
            return BadRequest(new { message = "El ID del negocio (businessId) es obligatorio." });
        }

        if (string.IsNullOrWhiteSpace(dto.InvoiceId))
        {
            return BadRequest(new { message = "El código de factura (invoiceId) es obligatorio." });
        }

        return await ProcessCreate(dto.BusinessId.Value, dto.InvoiceId, dto);
    }

    private async Task<ActionResult<PaymentDto>> ProcessCreate(int businessId, string invoiceId, PaymentCreateDto? dto)
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
