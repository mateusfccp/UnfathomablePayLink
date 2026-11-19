namespace UnfathomablePayLink.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Data transfer object for creating a new payment.
/// </summary>
public class PaymentCreateDto
{
    [Required(ErrorMessage = "El ID de la factura es obligatorio.")]
    public required string InvoiceId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El monto no puede ser negativo.")]
    public decimal Amount { get; set; }
}
