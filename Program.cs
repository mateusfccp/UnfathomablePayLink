using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
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
            Name = "X-Api-Key",
            In = ParameterLocation.Header,
            Description = "Insert your API Key to have access to the endpoints."
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
});

// Options
builder.Services.Configure<StreamHubOptions>(
    builder.Configuration.GetSection(StreamHubOptions.SectionName));
builder.Services.Configure<GobernanzaDigitalOptions>(
    builder.Configuration.GetSection(GobernanzaDigitalOptions.SectionName));

// Repositories
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddHttpClient<IInvoiceRepository, StreamHubInvoiceRepository>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<StreamHubOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/'));
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddHttpClient<IInvoiceRepository, GobernanzaDigitalInvoiceRepository>((serviceProvider, client) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<GobernanzaDigitalOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/'));
    client.Timeout = TimeSpan.FromSeconds(15);
});

// Services
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddHttpClient<IWebhookService, WebhookService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});

// Converters
builder.Services.AddSingleton<Converter<Payment, PaymentDto>>(_ => PaymentConverter.ToDto);

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
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

var app = builder.Build();

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
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "PayLink API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "Unfathomable PayLink — Apocalyptic API";
        c.HeadContent = """
            <link rel="icon" type="image/svg+xml" href="/swagger-custom/favicon.svg">
            <link rel="preconnect" href="https://fonts.googleapis.com">
            <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
            <link href="https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;600&family=Silkscreen:wght@400;700&display=swap" rel="stylesheet">
            """;
        c.InjectStylesheet("/swagger-custom/apocalypse.css");
        c.InjectJavascript("/swagger-custom/apocalypse.js");
    });
    app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous().ExcludeFromDescription();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" })).AllowAnonymous().ExcludeFromDescription();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

