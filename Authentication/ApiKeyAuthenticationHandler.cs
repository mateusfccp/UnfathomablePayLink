namespace UnfathomablePayLink.Authentication;

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Authenticates requests using the <c>X-Api-Key</c> header against registered <see cref="Business"/> records.
/// </summary>
public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>
    /// The authentication scheme name registered with ASP.NET Core authentication.
    /// </summary>
    public const string SchemeName = "ApiKey";

    /// <summary>
    /// The HTTP request header name carrying the business API key.
    /// </summary>
    public const string HeaderName = "X-Api-Key";

    private readonly IBusinessRepository _businessRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiKeyAuthenticationHandler"/> class.
    /// </summary>
    /// <param name="options">The monitor for authentication scheme options.</param>
    /// <param name="logger">The factory used to create loggers.</param>
    /// <param name="encoder">The URL encoder.</param>
    /// <param name="businessRepository">The repository used to resolve businesses from API keys.</param>
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IBusinessRepository businessRepository)
        : base(options, logger, encoder)
    {
        _businessRepository = businessRepository;
    }

    /// <inheritdoc />
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var extractedApiKey))
        {
            Logger.LogDebug(
                "Request {Method} {Path} did not include the {HeaderName} header.",
                Request.Method,
                Request.Path,
                HeaderName);
            return AuthenticateResult.NoResult();
        }

        var apiKey = extractedApiKey.ToString().Trim();
        if (string.IsNullOrEmpty(apiKey))
        {
            Logger.LogWarning(
                "Authentication failed for {Method} {Path}: {HeaderName} header was empty.",
                Request.Method,
                Request.Path,
                HeaderName);
            return AuthenticateResult.Fail("API Key is empty.");
        }

        var business = await _businessRepository.GetFromApiKeyAsync(apiKey);
        if (business is null)
        {
            Logger.LogWarning(
                "Authentication failed for {Method} {Path}: unrecognized API key.",
                Request.Method,
                Request.Path);
            return AuthenticateResult.Fail("Invalid API Key.");
        }

        // Preserve Business in HttpContext.Features for controllers reading HttpContext.Features.Get<Business>().
        Context.Features.Set(business);

        Logger.LogDebug(
            "Authenticated business {BusinessId} ('{BusinessName}') for {Method} {Path}.",
            business.Id,
            business.Name,
            Request.Method,
            Request.Path);

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

    /// <inheritdoc />
    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Logger.LogWarning(
            "Rejecting unauthenticated request {Method} {Path} with HTTP 401.",
            Request.Method,
            Request.Path);
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        await Response.WriteAsJsonAsync(new { message = "Unauthorized." });
    }

    /// <inheritdoc />
    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        var business = Context.Features.Get<Business>();
        Logger.LogWarning(
            "Rejecting forbidden request {Method} {Path} for business {BusinessId} with HTTP 403.",
            Request.Method,
            Request.Path,
            business?.Id);
        Response.StatusCode = StatusCodes.Status403Forbidden;
        await Response.WriteAsJsonAsync(new { message = "Forbidden." });
    }
}
