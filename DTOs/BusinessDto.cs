namespace UnfathomablePayLink.DTOs;

/// <summary>
/// Data transfer object representing a business entity (read/response DTO).
/// </summary>
public class BusinessDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Document { get; set; } = string.Empty;
}

/// <summary>
/// Alias for <see cref="BusinessDto"/> representing response data.
/// </summary>
public class BusinessReadDto : BusinessDto { }

/// <summary>
/// Alias for <see cref="BusinessDto"/> representing response data.
/// </summary>
public class BusinessResponseDto : BusinessDto { }
