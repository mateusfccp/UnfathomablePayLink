namespace UnfathomablePayLink.Repositories;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;
using UnfathomablePayLink.Options;

/// <summary>
/// Implements <see cref="IInvoiceRepository"/> for retrieving invoices from the external StreamHub API.
/// </summary>
public class StreamHubInvoiceRepository : IInvoiceRepository
{
    private static readonly SemaphoreSlim TokenLock = new(1, 1);
    private static string? _cachedToken;

    private readonly HttpClient _httpClient;
    private readonly StreamHubOptions _options;
    private readonly ILogger<StreamHubInvoiceRepository> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StreamHubInvoiceRepository"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured for StreamHub requests.</param>
    /// <param name="options">The StreamHub integration configuration options.</param>
    /// <param name="logger">The logger used to record StreamHub authentication and lookup diagnostics.</param>
    public StreamHubInvoiceRepository(
        HttpClient httpClient,
        IOptions<StreamHubOptions> options,
        ILogger<StreamHubInvoiceRepository> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public string ProviderKey => "streamhub";

    /// <inheritdoc />
    public async Task<IInvoice?> GetInvoiceByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var token = await GetTokenAsync(forceRefresh: false, cancellationToken);
        using var response = await SendGetInvoicesRequestAsync(token, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning("StreamHub returned 401 Unauthorized; refreshing JWT token and retrying.");
            response.Dispose();

            token = await GetTokenAsync(forceRefresh: true, cancellationToken);
            using var retryResponse = await SendGetInvoicesRequestAsync(token, cancellationToken);
            return await ParseInvoiceFromResponseAsync(retryResponse, id, cancellationToken);
        }

        return await ParseInvoiceFromResponseAsync(response, id, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendGetInvoicesRequestAsync(
        string token,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/payments/invoices");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await _httpClient.SendAsync(request, cancellationToken);
    }

    private async Task<IInvoice?> ParseInvoiceFromResponseAsync(
        HttpResponseMessage response,
        string invoiceId,
        CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "StreamHub invoice query failed with HTTP {StatusCode} while looking up '{InvoiceId}'.",
                (int)response.StatusCode,
                invoiceId);
            throw new HttpRequestException(
                $"StreamHub invoice query failed with status code {(int)response.StatusCode}.");
        }

        var items = await response.Content.ReadFromJsonAsync<List<StreamHubInvoiceItem>>(cancellationToken)
            ?? [];

        var match = items.FirstOrDefault(item =>
            string.Equals(item.CodigoPago, invoiceId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(item.Id.ToString(), invoiceId, StringComparison.OrdinalIgnoreCase));

        if (match is null)
        {
            return null;
        }

        var resolvedAmount = match.Monto;

        if (resolvedAmount is null)
        {
            _logger.LogWarning(
                "StreamHub invoice '{CodigoPago}' (Id={Id}) did not include an amount field; using DefaultFallbackAmount={FallbackAmount}.",
                match.CodigoPago,
                match.Id,
                _options.DefaultFallbackAmount);
        }

        var isPaid = string.Equals(match.Estado, "Pagado", StringComparison.OrdinalIgnoreCase);

        return new Invoice
        {
            Id = match.CodigoPago ?? match.Id.ToString(),
            Amount = resolvedAmount ?? _options.DefaultFallbackAmount,
            Description = $"StreamHub {match.Periodo ?? string.Empty} ({match.Estado ?? "Unknown"})".Trim(),
            DueDate = match.FechaVencimiento,
            IsPaid = isPaid
        };
    }

    private async Task<string> GetTokenAsync(bool forceRefresh, CancellationToken cancellationToken)
    {
        if (!forceRefresh && !string.IsNullOrWhiteSpace(_cachedToken))
        {
            return _cachedToken;
        }

        await TokenLock.WaitAsync(cancellationToken);
        try
        {
            if (!forceRefresh && !string.IsNullOrWhiteSpace(_cachedToken))
            {
                return _cachedToken;
            }

            if (string.IsNullOrWhiteSpace(_options.Email) || string.IsNullOrWhiteSpace(_options.Password))
            {
                throw new InvalidOperationException(
                    "StreamHub credentials (StreamHub:Email / StreamHub:Password) are not configured.");
            }

            _logger.LogDebug("Authenticating with StreamHub at {BaseUrl}/users/login.", _httpClient.BaseAddress);

            using var loginContent = JsonContent.Create(new StreamHubLoginRequest(_options.Email, _options.Password));
            await loginContent.LoadIntoBufferAsync(cancellationToken);
            using var loginResponse = await _httpClient.PostAsync(
                "/users/login",
                loginContent,
                cancellationToken);

            if (!loginResponse.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "StreamHub authentication failed with HTTP {StatusCode}.",
                    (int)loginResponse.StatusCode);
                throw new HttpRequestException(
                    $"StreamHub authentication failed with status code {(int)loginResponse.StatusCode}.");
            }

            var loginPayload = await loginResponse.Content.ReadFromJsonAsync<StreamHubLoginResponse>(cancellationToken);
            if (string.IsNullOrWhiteSpace(loginPayload?.Token))
            {
                _logger.LogError("StreamHub login response did not contain a JWT token.");
                throw new InvalidOperationException("StreamHub login response did not contain a JWT token.");
            }

            _cachedToken = loginPayload.Token;
            _logger.LogInformation("Successfully authenticated with StreamHub API.");
            return _cachedToken;
        }
        finally
        {
            TokenLock.Release();
        }
    }

    private sealed record StreamHubLoginRequest(
        [property: JsonPropertyName("email")] string Email,
        [property: JsonPropertyName("password")] string Password);

    private sealed class StreamHubLoginResponse
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }

    private sealed class StreamHubInvoiceItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("suscripcionId")]
        public int? SuscripcionId { get; set; }

        [JsonPropertyName("codigoPago")]
        public string? CodigoPago { get; set; }

        [JsonPropertyName("periodo")]
        public string? Periodo { get; set; }

        [JsonPropertyName("fechaVencimiento")]
        public DateTime? FechaVencimiento { get; set; }

        [JsonPropertyName("estado")]
        public string? Estado { get; set; }

        [JsonPropertyName("monto")]
        public decimal? Monto { get; set; }
    }
}
