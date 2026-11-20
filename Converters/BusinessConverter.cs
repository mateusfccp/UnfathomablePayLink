namespace UnfathomablePayLink.Converters;

using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Models;

/// <summary>
/// Provides <see cref="Converter{TInput, TOutput}"/> implementations for <see cref="Business"/> models.
/// </summary>
public static class BusinessConverter
{
    /// <summary>
    /// Converts a <see cref="Business"/> domain model into a <see cref="BusinessDto"/>.
    /// </summary>
    /// <param name="source">The source <see cref="Business"/> entity.</param>
    /// <returns>The mapped <see cref="BusinessDto"/> instance.</returns>
    public static BusinessDto ToDto(Business source)
    {
        return new BusinessDto
        {
            Id = source.Id,
            Name = source.Name,
            Document = source.Document,
            ApiKey = source.ApiKey,
            WebhookUrl = source.WebhookUrl,
            InvoiceProviderKey = source.InvoiceProviderKey
        };
    }
}
