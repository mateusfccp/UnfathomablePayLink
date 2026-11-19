namespace UnfathomablePayLink.Models;

using UnfathomablePayLink.Enums;

/// <summary>
/// Class <c>Payment</c> models a payment of a business' invoice.
/// </summary>
sealed public class Payment
{
    public int Id { get; set; }
    public required Business Business { get; set; }
    public required string InvoiceId { get; set; }
    public required decimal Amount { get; set; }
    public required PaymentState State { get; set; }
    public DateTime? PaymentDate { get; set; }
}
