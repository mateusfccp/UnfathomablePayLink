using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using UnfathomablePayLink.Authentication;
using UnfathomablePayLink.Context;
using UnfathomablePayLink.Converters;
using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;
using UnfathomablePayLink.Options;
using UnfathomablePayLink.Repositories;
using UnfathomablePayLink.Routing;
using UnfathomablePayLink.Services;

var builder = WebApplication.CreateBuilder(args);

if (Environment.GetEnvironmentVariable("PORT") is { Length: > 0 } port)
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(
        new RouteTokenTransformerConvention(new LowercaseParameterTransformer()));
});

builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "PayLink API";
        document.Info.Version = "v1";
        document.Info.Description = "Restful API for the PayLink service.";

        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            Name = ApiKeyAuthenticationHandler.HeaderName,
            In = ParameterLocation.Header,
            Description = "Insert your Business or Admin API Key to access endpoints."
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["ApiKeyAuth"] = securityScheme;
        document.RegisterComponents();

        // En v2.x se referencia por OpenApiSecuritySchemeReference con el documento anfitrión
        var schemeRef = new OpenApiSecuritySchemeReference("ApiKeyAuth", document);
        var requirement = new OpenApiSecurityRequirement();
        requirement.Add(schemeRef, new List<string>());

        // En v2.x la propiedad es document.Security
        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(requirement);

        document.Webhooks ??= new Dictionary<string, IOpenApiPathItem>();
        document.Webhooks["paymentProcessed"] = CreatePaymentProcessedWebhookPathItem(document);

        if (document.Paths.TryGetValue("/api/webhook", out var webhookPath) &&
            webhookPath.Operations is not null &&
            webhookPath.Operations.TryGetValue(HttpMethod.Put, out var putWebhookOperation))
        {
            var callback = new OpenApiCallback();
            callback.AddPathItem(
                RuntimeExpression.Build("{$request.body#/url}"),
                CreatePaymentProcessedWebhookPathItem(document));

            putWebhookOperation.Callbacks ??= new Dictionary<string, IOpenApiCallback>();
            putWebhookOperation.Callbacks["paymentProcessed"] = callback;
        }

        return Task.CompletedTask;

        static OpenApiPathItem CreatePaymentProcessedWebhookPathItem(OpenApiDocument document)
        {
            var operation = new OpenApiOperation
            {
                Summary = "Payment processed notification",
                Description = "Sent via HTTP POST by PayLink to the business's configured webhook URL whenever a payment transitions to APPROVED or REJECTED. Uses Content-Type: application/json, a 10-second timeout, and a single delivery attempt (no retries).",
                Security = new List<OpenApiSecurityRequirement>(),
                RequestBody = new OpenApiRequestBody
                {
                    Required = true,
                    Description = "The processed payment transaction payload.",
                    Content = new Dictionary<string, IOpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchemaReference("PaymentDto", document),
                            Example = new JsonObject
                            {
                                ["id"] = 42,
                                ["invoiceId"] = "INV-001",
                                ["amount"] = 1500.50m,
                                ["state"] = "APPROVED",
                                ["paymentDate"] = "2026-11-19T12:00:00Z"
                            }
                        }
                    }
                },
                Responses = new OpenApiResponses
                {
                    ["200"] = new OpenApiResponse
                    {
                        Description = "Return any 2xx HTTP status code to acknowledge receipt of the webhook notification."
                    }
                }
            };

            var pathItem = new OpenApiPathItem();
            pathItem.AddOperation(HttpMethod.Post, operation);
            return pathItem;
        }
    });
});

// Db context
builder.Services.AddDbContext<PayLinkContext>(options =>
{
    options.UseSqlite(
	builder.Configuration.GetConnectionString("PayLinkDb")
    );
    options.ConfigureWarnings(warnings =>
                  warnings.Ignore(RelationalEventId.NonTransactionalMigrationOperationWarning));
});

// Options
builder.Services.Configure<AdminOptions>(
    builder.Configuration.GetSection(AdminOptions.SectionName));
builder.Services.Configure<StreamHubOptions>(
    builder.Configuration.GetSection(StreamHubOptions.SectionName));
builder.Services.Configure<GobernanzaDigitalOptions>(
    builder.Configuration.GetSection(GobernanzaDigitalOptions.SectionName));

builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<IPostConfigureOptions<OpenApiOptions>, AdminOpenApiOptionsSetup>();

// Repositories
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddHttpClient<StreamHubInvoiceRepository>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<StreamHubOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/'));
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddTransient<IInvoiceRepository>(
    serviceProvider => serviceProvider.GetRequiredService<StreamHubInvoiceRepository>());

builder.Services.AddHttpClient<GobernanzaDigitalInvoiceRepository>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<GobernanzaDigitalOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/'));
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddTransient<IInvoiceRepository>(
    serviceProvider => serviceProvider.GetRequiredService<GobernanzaDigitalInvoiceRepository>());

// Services
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddHttpClient<IWebhookService, WebhookService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Converters
builder.Services.AddSingleton<Converter<Business, BusinessDto>>(_ => BusinessConverter.ToDto);
builder.Services.AddSingleton<Converter<Payment, PaymentDto>>(_ => PaymentConverter.ToDto);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// Route options
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

// Authentication & Authorization (API Key required by default on all endpoints)
builder.Services
    .AddAuthentication(ApiKeyAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationHandler.SchemeName,
        null);

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.AdminOnly, policy =>
        policy.RequireRole(AuthorizationPolicies.AdminRole))
    .AddPolicy(AuthorizationPolicies.BusinessOnly, policy =>
        policy.RequireRole(AuthorizationPolicies.BusinessRole))
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseForwardedHeaders();

if (!EF.IsDesignTime)
{
    await DatabaseInitializer.InitializeAsync(app.Services, app.Configuration);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.MapOpenApi()
        .AllowAnonymous()
        .AddEndpointFilter(async (context, next) =>
        {
            context.HttpContext.Response.Headers.CacheControl = "no-store";
            context.HttpContext.Response.Headers.Vary = ApiKeyAuthenticationHandler.HeaderName;
            return await next(context);
        });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "PayLink API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Unfathomable PayLink — Apocalyptic API";
        c.EnablePersistAuthorization();
        c.UseRequestInterceptor("""
            (request) => {
                if (request && typeof request.url === "string" && request.url.indexOf("/openapi/") !== -1) {
                    try {
                        let apiKey = null;
                        if (window.ui && window.ui.authSelectors && typeof window.ui.authSelectors.authorized === "function") {
                            const uiAuth = window.ui.authSelectors.authorized();
                            if (uiAuth && typeof uiAuth.getIn === "function") {
                                apiKey = uiAuth.getIn(["ApiKeyAuth", "value"]) || null;
                            }
                        } else {
                            const raw = window.localStorage.getItem("authorized");
                            if (raw) {
                                const parsed = JSON.parse(raw);
                                apiKey = parsed && parsed.ApiKeyAuth ? parsed.ApiKeyAuth.value || null : null;
                            }
                        }
                        if (apiKey) {
                            request.headers = request.headers || {};
                            request.headers["X-Api-Key"] = apiKey;
                        }
                    } catch (_) {}
                }
                return request;
            }
            """);
        c.HeadContent = """
            <link rel="icon" type="image/svg+xml" href="/swagger-custom/favicon.svg">
            <link rel="preconnect" href="https://fonts.googleapis.com">
            <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
            <link href="https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;600&family=Silkscreen:wght@400;700&display=swap" rel="stylesheet">
            """;
        c.InjectStylesheet("/swagger-custom/apocalypse.css");
        c.InjectJavascript("/swagger-custom/apocalypse.js");
    });
    app.MapMethods("/", ["GET", "HEAD"], () => Results.Redirect("/swagger")).AllowAnonymous().ExcludeFromDescription();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).AllowAnonymous().ExcludeFromDescription();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(
            exception,
            "HTTP 500 Internal Server Error em {Method} {Path} | TraceId: {TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.TraceIdentifier);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Erro interno no servidor",
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
