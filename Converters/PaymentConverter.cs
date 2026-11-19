namespace UnfathomablePayLink.Converters;

using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Models;

/// <summary>
/// Provides <see cref="Converter{TInput, TOutput}"/> implementations for <see cref="Payment"/> models.
/// </summary>
public static class PaymentConverter
{
    /// <summary>
    /// Converts a <see cref="Payment"/> domain model into a <see cref="PaymentDto"/>.
    /// </summary>
    /// <param name="source">The source <see cref="Payment"/> entity.</param>
    /// <returns>The mapped <see cref="PaymentDto"/> instance.</returns>
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
