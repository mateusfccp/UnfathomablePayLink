namespace UnfathomablePayLink.Authentication;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;

/// <summary>
/// Filters administrator-only endpoints out of the generated OpenAPI specification unless the caller is authenticated as an administrator.
/// </summary>
public sealed class AdminOpenApiOptionsSetup : IPostConfigureOptions<OpenApiOptions>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminOpenApiOptionsSetup"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The accessor used to inspect the current HTTP request principal.</param>
    public AdminOpenApiOptionsSetup(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public void PostConfigure(string? name, OpenApiOptions options)
    {
        var previousShouldInclude = options.ShouldInclude;
        options.ShouldInclude = description =>
        {
            if (!previousShouldInclude(description))
            {
                return false;
            }

            var isAdminOnly = description.ActionDescriptor.EndpointMetadata
                .OfType<IAuthorizeData>()
                .Any(auth => string.Equals(auth.Policy, AuthorizationPolicies.AdminOnly, StringComparison.Ordinal));

            if (!isAdminOnly)
            {
                return true;
            }

            return _httpContextAccessor.HttpContext?.User.IsInRole(AuthorizationPolicies.AdminRole) == true;
        };
    }
}
