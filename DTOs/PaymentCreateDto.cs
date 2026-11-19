namespace UnfathomablePayLink.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Data transfer object for creating a new payment.
/// </summary>
public class PaymentCreateDto
{
    [Required(ErrorMessage = "El ID de la factura es obligatorio.")]
    public required string InvoiceId { get; set; }
}
