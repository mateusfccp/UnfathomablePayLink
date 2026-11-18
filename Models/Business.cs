namespace UnfathomablePayLink.Models;

/// <summary>
/// Class <c>Business</c> models a business that manages payments.
/// </summary>
sealed public class Business
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Document { get; set; }
}
