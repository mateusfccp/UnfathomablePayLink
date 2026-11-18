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
            BusinessId = payment.Business?.Id ?? 0,
            BusinessName = payment.Business?.Name ?? string.Empty,
            Business = payment.Business?.ToDto(),
            InvoiceId = payment.InvoiceId,
            Amount = payment.amount,
            State = payment.state,
            PaymentDate = payment.PaymentDate
        };
    }

    public static InvoiceDto ToDto(this Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            Amount = invoice.Amount,
            Description = invoice.Description,
            DueDate = invoice.DueDate,
            IsPaid = invoice.IsPaid
        };
    }
}
