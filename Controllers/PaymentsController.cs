namespace UnfathomablePayLink.Controllers;

using Microsoft.AspNetCore.Mvc;
using UnfathomablePayLink.Exceptions;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentsController(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    /// <summary>
    /// GET api/payments/{id}
    /// Retrieves a specific payment by its primary key ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Payment>> GetById(int id)
    {
        var payment = await _paymentRepository.GetPaymentById(id);
        if (payment == null)
        {
            return NotFound(new { message = $"Payment with ID {id} was not found." });
        }

        return Ok(payment);
    }

    /// <summary>
    /// GET api/payments/business/{businessId}/invoice/{invoiceId}
    /// Retrieves all payment attempts for a given business invoice.
    /// </summary>
    [HttpGet("business/{businessId:int}/invoice/{invoiceId}")]
    public async Task<ActionResult<List<Payment>>> GetPaymentsByInvoice(int businessId, string invoiceId)
    {
        var payments = await _paymentRepository.GetPaymentsByInvoiceId(businessId, invoiceId);
        return Ok(payments);
    }

    /// <summary>
    /// GET api/payments/business/{businessId}/invoice/{invoiceId}/latest
    /// Retrieves the most recent payment attempt for a given business invoice.
    /// </summary>
    [HttpGet("business/{businessId:int}/invoice/{invoiceId}/latest")]
    public async Task<ActionResult<Payment>> GetLatestPaymentByInvoice(int businessId, string invoiceId)
    {
        var payment = await _paymentRepository.GetLatestPaymentByInvoiceId(businessId, invoiceId);
        if (payment == null)
        {
            return NotFound(new { message = $"No payment records found for Business {businessId} and Invoice {invoiceId}." });
        }

        return Ok(payment);
    }

    /// <summary>
    /// POST api/payments/business/{businessId}/invoice/{invoiceId}
    /// Attempts to create a new payment for a business invoice.
    /// </summary>
    [HttpPost("business/{businessId:int}/invoice/{invoiceId}")]
    public async Task<IActionResult> CreatePayment(int businessId, string invoiceId)
    {
        try
        {
            await _paymentRepository.CreatePaymentForInvoiceId(businessId, invoiceId);
            return StatusCode(StatusCodes.Status201Created, new { message = "Payment created successfully." });
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
    }
}