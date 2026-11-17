namespace UnfathomablePayLink.Models;

/// <summary>
/// Class <c>Invoice</c> represents an invoice or bill queried from a business.
/// Implements <c>IInvoice&lt;string&gt;</c>.
/// </summary>
public class Invoice : IInvoice<string>
{
    public required string Id { get; set; }
    public required decimal Amount { get; set; }
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsPaid { get; set; }
}
