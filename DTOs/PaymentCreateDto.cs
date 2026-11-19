namespace UnfathomablePayLink.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Data transfer object for requesting a new payment transaction.
/// </summary>
public class PaymentCreateDto
{
    /// <summary>
    /// Gets or sets the identifier of the external invoice to pay.
    /// </summary>
    [Required(ErrorMessage = "El ID de la factura es obligatorio.")]
    public required string InvoiceId { get; set; }
}
