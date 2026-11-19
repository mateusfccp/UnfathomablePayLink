using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using UnfathomablePayLink.Authentication;
using UnfathomablePayLink.Context;
using UnfathomablePayLink.Converters;
using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;
using UnfathomablePayLink.Repositories;
using UnfathomablePayLink.Routing;
using UnfathomablePayLink.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(
        new RouteTokenTransformerConvention(new LowercaseParameterTransformer()));
});

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "PayLink API";
        document.Info.Version = "v1";
        document.Info.Description = "API RESTful para pasarela de pagos PayLink (Programación II)";

        var securityScheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            Name = "X-Api-Key",
            In = ParameterLocation.Header,
            Description = "Ingrese su API Key asignada para operar sobre la pasarela de pagos"
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

        return Task.CompletedTask;
    });
});

// Db context
builder.Services.AddDbContext<PayLinkContext>(options =>
{
    options.UseSqlite(
	builder.Configuration.GetConnectionString("PayLinkDb")
    );
});

// Repositories
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();

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

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi().AllowAnonymous();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "PayLink API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

