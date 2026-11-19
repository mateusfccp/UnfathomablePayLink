namespace UnfathomablePayLink.DTOs;

using UnfathomablePayLink.Enums;

/// <summary>
/// Data transfer object representing a payment transaction (read/response DTO).
/// Projects related business data to avoid cycles and provide a clean API contract.
/// </summary>
public class PaymentDto
{
    public int Id { get; set; }
    public string InvoiceId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public PaymentState State { get; set; }
    public DateTime? PaymentDate { get; set; }
}
