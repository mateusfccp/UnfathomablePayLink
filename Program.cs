using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using UnfathomablePayLink.Context;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;
using UnfathomablePayLink.Repositories;
using UnfathomablePayLink.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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
        document.Components.SecuritySchemes.Add("ApiKeyAuth", securityScheme);

        // En v2.x se referencia por OpenApiSecuritySchemeReference
        var schemeRef = new OpenApiSecuritySchemeReference("ApiKeyAuth");
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
builder.Services.AddScoped<IInvoiceRepository<Invoice, string>, MockInvoiceRepository>();

// Services
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "PayLink API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Endpoints
app.MapControllers();

app.Run();

