namespace UnfathomablePayLink.Authentication;

/// <summary>
/// Defines role and authorization policy constants used across PayLink endpoints.
/// </summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Role assigned to requests authenticated with the administrator API key.
    /// </summary>
    public const string AdminRole = "Admin";

    /// <summary>
    /// Role assigned to requests authenticated with a registered business API key.
    /// </summary>
    public const string BusinessRole = "Business";

    /// <summary>
    /// Policy requiring administrator authentication.
    /// </summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>
    /// Policy requiring business authentication.
    /// </summary>
    public const string BusinessOnly = "BusinessOnly";
}
