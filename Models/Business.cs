using Microsoft.EntityFrameworkCore;

namespace UnfathomablePayLink.Models;

/// <summary>
/// Class <c>Business</c> models a business that manages payments.
/// </summary>
[Index(nameof(Document), IsUnique = true)]
[Index(nameof(ApiKey), IsUnique = true)]
sealed public class Business
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Document { get; set; }
    public required string ApiKey { get; set; }
}
