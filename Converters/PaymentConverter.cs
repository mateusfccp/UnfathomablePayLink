namespace UnfathomablePayLink.Converters;

using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Models;

/// <summary>
/// Provides <see cref="Converter{TInput, TOutput}"/> implementations for <see cref="Payment"/> models.
/// </summary>
public static class PaymentConverter
{
    public static PaymentDto ToDto(Payment source)
    {
        return new PaymentDto
        {
            Id = source.Id,
            InvoiceId = source.InvoiceId,
            Amount = source.Amount,
            State = source.State,
            PaymentDate = source.PaymentDate
        };
    }
}
