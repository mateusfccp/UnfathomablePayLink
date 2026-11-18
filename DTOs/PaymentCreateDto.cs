namespace UnfathomablePayLink.DTOs;

using System.ComponentModel.DataAnnotations;
using UnfathomablePayLink.Enums;

/// <summary>
/// Data transfer object for creating a new payment.
/// </summary>
public class PaymentCreateDto
{
    [Range(0, double.MaxValue, ErrorMessage = "El monto no puede ser negativo.")]
    public decimal? Amount { get; set; }

    public PaymentState? State { get; set; }
}

/// <summary>
/// Alias for <see cref="PaymentCreateDto"/>.
/// </summary>
public class CreatePaymentDto : PaymentCreateDto { }
