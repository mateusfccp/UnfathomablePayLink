namespace UnfathomablePayLink.DTOs;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Data transfer object for registering a new business entity.
/// Contains input validation attributes according to data validation guidelines.
/// </summary>
public class BusinessCreateDto
{
    [Required(ErrorMessage = "El nombre del negocio es obligatorio.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "El documento/CUIT es obligatorio.")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "El documento debe tener entre 1 y 20 caracteres.")]
    public string Document { get; set; } = string.Empty;
}
