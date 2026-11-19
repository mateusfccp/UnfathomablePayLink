namespace UnfathomablePayLink.Models;

using UnfathomablePayLink.Interfaces;

/// <summary>
/// Class <c>Invoice</c> represents an invoice or bill queried from a business.
/// Implements <c>IInvoice</c>.
/// </summary>
public class Invoice : IInvoice
{
    public required string Id { get; set; }
    public required decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsPaid { get; set; }
}
