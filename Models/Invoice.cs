namespace UnfathomablePayLink.Models;

using UnfathomablePayLink.Interfaces;

/// <summary>
/// Represents an invoice or bill retrieved from a <see cref="Business"/> billing provider.
/// Implements <see cref="IInvoice"/>.
/// </summary>
public class Invoice : IInvoice
{
    /// <inheritdoc />
    public required string Id { get; set; }

    /// <inheritdoc />
    public required decimal Amount { get; set; }

    /// <summary>
    /// Gets or sets an optional human-readable description of the invoice.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the optional due date of the invoice.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the invoice has already been marked as paid externally.
    /// </summary>
    public bool IsPaid { get; set; }
}
