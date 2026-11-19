namespace UnfathomablePayLink.DTOs;

using UnfathomablePayLink.Models;

/// <summary>
/// Extension methods for mapping between domain models and DTOs.
/// </summary>
public static class MappingExtensions
{
    public static BusinessDto ToDto(this Business business)
    {
        return new BusinessDto
        {
            Id = business.Id,
            Name = business.Name,
            Document = business.Document
        };
    }

    public static Business ToEntity(this BusinessCreateDto dto)
    {
        return new Business
        {
            Name = dto.Name,
            Document = dto.Document
        };
    }

    public static PaymentDto ToDto(this Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            InvoiceId = payment.InvoiceId,
            Amount = payment.amount,
            State = payment.state,
            PaymentDate = payment.PaymentDate
        };
    }

    public static Payment ToEntity(this PaymentCreateDto dto, Business business)
    {
        return new Payment
        {
            Business = business,
            InvoiceId = dto.InvoiceId,
            amount = dto.Amount,
            state = Enums.PaymentState.Pending,
            PaymentDate = DateTime.UtcNow
        };
    }
}
