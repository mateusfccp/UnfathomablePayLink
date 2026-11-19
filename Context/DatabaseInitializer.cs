namespace UnfathomablePayLink.Context;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using UnfathomablePayLink.Enums;
using UnfathomablePayLink.Models;

/// <summary>
/// Applies pending Entity Framework Core migrations and seeds an initial test business with sample payments when empty.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Ensures the SQLite directory exists, applies database migrations, and optionally seeds sample data.
    /// </summary>
    /// <param name="services">The root application service provider.</param>
    /// <param name="configuration">The application configuration containing seed settings.</param>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<PayLinkContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DatabaseInitializer));

        var connectionString = context.Database.GetConnectionString();
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            var builder = new SqliteConnectionStringBuilder(connectionString);
            if (!string.IsNullOrWhiteSpace(builder.DataSource) &&
                !string.Equals(builder.DataSource, ":memory:", StringComparison.OrdinalIgnoreCase))
            {
                var directory = Path.GetDirectoryName(builder.DataSource);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                    logger.LogDebug("Ensured SQLite database directory '{Directory}' exists.", directory);
                }
            }
        }

        logger.LogDebug("Checking and applying pending database migrations...");
        await context.Database.MigrateAsync();

        if (!configuration.GetValue("Seed:Enabled", true))
        {
            logger.LogDebug("Database test data seeding is disabled via configuration.");
            return;
        }

        if (await context.Businesses.AnyAsync())
        {
            logger.LogDebug("Existing business records found; skipping test data seeding.");
            return;
        }

        var name = GetNonEmptyOrDefault(configuration["Seed:TestBusinessName"], "Testing");
        var document = GetNonEmptyOrDefault(configuration["Seed:TestBusinessDocument"], "00-00000000-0");
        var apiKey = GetNonEmptyOrDefault(configuration["Seed:TestBusinessApiKey"], "testing");

        var testBusiness = new Business
        {
            Name = name,
            Document = document,
            ApiKey = apiKey,
            WebhookUrl = null,
            InvoiceProviderKey = "mock"
        };

        var now = DateTime.UtcNow;
        var samplePayments = new List<Payment>
        {
            new()
            {
                Business = testBusiness,
                InvoiceId = "INV-001",
                Amount = 1500.50m,
                State = PaymentState.Rejected,
                PaymentDate = now.AddDays(-2)
            },
            new()
            {
                Business = testBusiness,
                InvoiceId = "INV-001",
                Amount = 1500.50m,
                State = PaymentState.Approved,
                PaymentDate = now.AddDays(-1)
            },
            new()
            {
                Business = testBusiness,
                InvoiceId = "INV-003",
                Amount = 1000.00m,
                State = PaymentState.Rejected,
                PaymentDate = now.AddHours(-6)
            }
        };

        var seedBusinesses = new List<Business>
        {
            new()
            {
                Name = "StreamHub",
                Document = "01-00010001-1",
                ApiKey = "key_R7bhz3nWggRC1POIDRBJWbWIX71XnhOl",
                InvoiceProviderKey = "streamhub"
            },
            new()
            {
                Name = "Gobernanza Digital",
                Document = "02-00020002-2",
                ApiKey = "key_i3hqHJILK9GEK6EVw4aqQwetPJCY1BWa",
                InvoiceProviderKey = "mock"
            }
        };

        await context.Businesses.AddAsync(testBusiness);
        await context.Businesses.AddRangeAsync(seedBusinesses);
        await context.Payments.AddRangeAsync(samplePayments);
        await context.SaveChangesAsync();

        logger.LogInformation(
            "Seeded test business '{BusinessName}' ({Document}) with {PaymentCount} sample payments.",
            testBusiness.Name,
            testBusiness.Document,
            samplePayments.Count);
    }

    private static string GetNonEmptyOrDefault(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}
