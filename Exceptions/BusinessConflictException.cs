namespace UnfathomablePayLink.Exceptions;

using UnfathomablePayLink.Models;

/// <summary>
/// Exception thrown when a <see cref="Business"/> operation conflicts with existing data or relational constraints.
/// </summary>
public class BusinessConflictException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessConflictException"/> class with a descriptive message.
    /// </summary>
    /// <param name="message">The conflict error message.</param>
    public BusinessConflictException(string message)
        : base(message)
    {
    }
}
