namespace UnfathomablePayLink.DTOs;

using UnfathomablePayLink.Enums;

/// <summary>
/// Data transfer object representing a payment transaction (read/response DTO).
/// Projects related business data to avoid cycles and provide a clean API contract.
/// </summary>
public class PaymentDto
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public BusinessDto? Business { get; set; }
    public string InvoiceId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentState State { get; set; }
    public DateTime? PaymentDate { get; set; }
}

/// <summary>
/// Alias for <see cref="PaymentDto"/> representing response data.
/// </summary>
public class PaymentReadDto : PaymentDto { }

/// <summary>
/// Alias for <see cref="PaymentDto"/> representing response data.
/// </summary>
public class PaymentResponseDto : PaymentDto { }
