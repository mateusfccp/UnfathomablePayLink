namespace UnfathomablePayLink.Models;

using UnfathomablePayLink.Enums;

/// <summary>
/// Class <c>Payment</c> models a payment of a business' invoice.
/// </summary>
sealed public class Payment
{
    public required int Id { get; set; }
    public required Business Business { get; set; }
    public required string InvoiceId { get; set; }
    public required decimal amount { get; set; }
    public required PaymentState state { get; set; }
    public DateTime? PaymentDate { get; set; }
}
