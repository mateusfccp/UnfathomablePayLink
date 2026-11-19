namespace UnfathomablePayLink.Repositories;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Implements <see cref="IInvoiceRepository"/> for retrieving municipal bills from the Gobernanza Digital API.
/// </summary>
public class GobernanzaDigitalInvoiceRepository : IInvoiceRepository
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GobernanzaDigitalInvoiceRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GobernanzaDigitalInvoiceRepository"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured for Gobernanza Digital requests.</param>
    /// <param name="logger">The logger used to record external bill lookup diagnostics.</param>
    public GobernanzaDigitalInvoiceRepository(
        HttpClient httpClient,
        ILogger<GobernanzaDigitalInvoiceRepository> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public string ProviderKey => "gobernanza-digital";

    /// <inheritdoc />
    public async Task<IInvoice?> GetInvoiceByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var encodedId = Uri.EscapeDataString(id);
        _logger.LogDebug(
            "Querying Gobernanza Digital bill '{BillId}' at {BaseAddress}bills/{EncodedId}.",
            id,
            _httpClient.BaseAddress,
            encodedId);

        using var response = await _httpClient.GetAsync($"/bills/{encodedId}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogDebug("Gobernanza Digital bill '{BillId}' was not found (HTTP 404).", id);
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Gobernanza Digital bill query for '{BillId}' failed with HTTP {StatusCode}.",
                id,
                (int)response.StatusCode);
            throw new HttpRequestException(
                $"Gobernanza Digital bill query failed with status code {(int)response.StatusCode}.");
        }

        var bill = await response.Content.ReadFromJsonAsync<GobernanzaBillDto>(cancellationToken);
        if (bill is null)
        {
            return null;
        }

        var description = string.IsNullOrWhiteSpace(bill.ServiceName)
            ? bill.Period
            : string.IsNullOrWhiteSpace(bill.Period)
                ? bill.ServiceName
                : $"{bill.ServiceName} ({bill.Period})";

        return new Invoice
        {
            Id = string.IsNullOrWhiteSpace(bill.BillId) ? id : bill.BillId,
            Amount = bill.Amount,
            Description = description,
            DueDate = bill.DueDate,
            IsPaid = string.Equals(bill.Status, "Pagada", StringComparison.OrdinalIgnoreCase)
        };
    }

    private sealed class GobernanzaBillDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("billId")]
        public string? BillId { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("dueDate")]
        public DateTime? DueDate { get; set; }

        [JsonPropertyName("period")]
        public string? Period { get; set; }

        [JsonPropertyName("serviceName")]
        public string? ServiceName { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }
    }
}
