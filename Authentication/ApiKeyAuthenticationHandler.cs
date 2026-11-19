namespace UnfathomablePayLink.Authentication;

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using UnfathomablePayLink.Interfaces;

/// <summary>
/// Authenticates requests using the <c>X-Api-Key</c> header against registered businesses.
/// </summary>
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    private readonly IBusinessRepository _businessRepository;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IBusinessRepository businessRepository)
        : base(options, logger, encoder)
    {
        _businessRepository = businessRepository;
    }

    // Reads X-Api-Key, resolves the business, and builds the authenticated identity (ClaimsPrincipal).
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var extractedApiKey))
        {
            return AuthenticateResult.NoResult();
        }

        var apiKey = extractedApiKey.ToString().Trim();
        if (string.IsNullOrEmpty(apiKey))
        {
            return AuthenticateResult.Fail("API Key is empty.");
        }

        var business = await _businessRepository.GetFromApiKeyAsync(apiKey);
        if (business is null)
        {
            return AuthenticateResult.Fail("Invalid API Key.");
        }

        // Preserve Business in HttpContext.Features for controllers reading HttpContext.Features.Get<Business>().
        Context.Features.Set(business);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, business.Id.ToString()),
            new Claim(ClaimTypes.Name, business.Name),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }

    // Called by UseAuthorization when an unauthenticated request hits a protected endpoint (HTTP 401).
    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        await Response.WriteAsJsonAsync(new { message = "Unauthorized." });
    }

    // Called by Forbid() or failing authorization policies when authenticated (HTTP 403).
    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        await Response.WriteAsJsonAsync(new { message = "Forbidden." });
    }
}
