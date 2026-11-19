namespace UnfathomablePayLink.Services;

using System.Net.Http.Json;
using UnfathomablePayLink.DTOs;
using UnfathomablePayLink.Enums;
using UnfathomablePayLink.Interfaces;
using UnfathomablePayLink.Models;

/// <summary>
/// Implements <see cref="IWebhookService"/> to deliver HTTP JSON webhook notifications to businesses.
/// </summary>
public class WebhookService : IWebhookService
{
    private const int MaxResponseSnippetLength = 500;

    private readonly HttpClient _httpClient;
    private readonly Converter<Payment, PaymentDto> _paymentConverter;
    private readonly ILogger<WebhookService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebhookService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to dispatch webhook requests.</param>
    /// <param name="paymentConverter">The converter used to map <see cref="Payment"/> entities to <see cref="PaymentDto"/> payloads.</param>
    /// <param name="logger">The logger used to record webhook delivery outcomes.</param>
    public WebhookService(
        HttpClient httpClient,
        Converter<Payment, PaymentDto> paymentConverter,
        ILogger<WebhookService> logger)
    {
        _httpClient = httpClient;
        _paymentConverter = paymentConverter;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task NotifyPaymentProcessedAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (payment.State is not (PaymentState.Approved or PaymentState.Rejected))
        {
            _logger.LogWarning(
                "Skipping webhook notification for payment {PaymentId} because state {State} is not terminal.",
                payment.Id,
                payment.State);
            return;
        }

        var webhookUrl = payment.Business?.WebhookUrl;
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            _logger.LogDebug(
                "No webhook URL configured for business {BusinessId}; skipping notification for payment {PaymentId}.",
                payment.Business?.Id,
                payment.Id);
            return;
        }

        var trimmedUrl = webhookUrl.Trim();
        if (!Uri.TryCreate(trimmedUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            _logger.LogWarning(
                "Invalid webhook URL '{WebhookUrl}' configured for business {BusinessId}; skipping notification for payment {PaymentId}.",
                trimmedUrl,
                payment.Business?.Id,
                payment.Id);
            return;
        }

        var payload = _paymentConverter(payment);

        try
        {
            using var content = JsonContent.Create(payload);
            await content.LoadIntoBufferAsync(cancellationToken);
            using var response = await _httpClient.PostAsync(uri, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Webhook delivered for payment {PaymentId} ({State}) to {WebhookUrl} with status code {StatusCode}.",
                    payment.Id,
                    payment.State,
                    uri,
                    (int)response.StatusCode);
                return;
            }

            var responseSnippet = await ReadTruncatedResponseAsync(response, cancellationToken);
            _logger.LogWarning(
                "Webhook delivery failed for payment {PaymentId} ({State}) to {WebhookUrl} with status code {StatusCode}. Response: {ResponseSnippet}",
                payment.Id,
                payment.State,
                uri,
                (int)response.StatusCode,
                responseSnippet);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Webhook delivery timed out for payment {PaymentId} ({State}) to {WebhookUrl}.",
                payment.Id,
                payment.State,
                uri);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "HTTP request error while delivering webhook for payment {PaymentId} ({State}) to {WebhookUrl}.",
                payment.Id,
                payment.State,
                uri);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while delivering webhook for payment {PaymentId} ({State}) to {WebhookUrl}.",
                payment.Id,
                payment.State,
                uri);
        }
    }

    private static async Task<string> ReadTruncatedResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrEmpty(body))
            {
                return string.Empty;
            }

            return body.Length <= MaxResponseSnippetLength
                ? body
                : string.Concat(body.AsSpan(0, MaxResponseSnippetLength), "...");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return "<unreadable>";
        }
    }
}
