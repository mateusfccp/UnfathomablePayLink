
using Microsoft.EntityFrameworkCore;
using unfathomable_pay_link.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Db context
builder.Services.AddDbContext<PayLinkContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("PayLinkDb")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

