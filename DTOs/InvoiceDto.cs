namespace UnfathomablePayLink.DTOs;

/// <summary>
/// Data transfer object representing external invoice details.
/// </summary>
public class InvoiceDto
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsPaid { get; set; }
}

/// <summary>
/// Alias for <see cref="InvoiceDto"/> representing response data.
/// </summary>
public class InvoiceReadDto : InvoiceDto { }

/// <summary>
/// Alias for <see cref="InvoiceDto"/> representing response data.
/// </summary>
public class InvoiceResponseDto : InvoiceDto { }
