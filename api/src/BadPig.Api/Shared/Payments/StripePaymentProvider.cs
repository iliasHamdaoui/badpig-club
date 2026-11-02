using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BadPig.Shared.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BadPig.Shared.Payments;

/// <summary>
/// Stripe Checkout: a hosted payment page (cards, Apple Pay, Google Pay according to the Stripe dashboard settings).
/// Stripe notifies the application with signed webhooks; the signature is checked here.
/// </summary>
internal sealed class StripePaymentProvider(HttpClient http, IOptions<StripeOptions> options, IDateTimeProvider clock,
    ILogger<StripePaymentProvider> log) : IPaymentProvider
{
    // Stripe refuses a signature older than this (replay protection).
    private static readonly TimeSpan SignatureTolerance = TimeSpan.FromMinutes(5);

    private StripeOptions Options => options.Value;

    public async Task<Result<PaymentSession>> CreateSessionAsync(PaymentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Options.SecretKey))
            return Error.Unexpected("Le paiement en ligne n'est pas configuré.");

        var form = new List<KeyValuePair<string, string>>
        {
            new("mode", "payment"),
            new("success_url", request.SuccessUrl),
            new("cancel_url", request.CancelUrl),
            new("client_reference_id", request.OrderId.ToString()),
            new("metadata[order_id]", request.OrderId.ToString()),
            new("locale", "fr"),
            new("expires_at", request.ExpiresAt.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)),
        };
        if (!string.IsNullOrWhiteSpace(request.CustomerEmail))
            form.Add(new("customer_email", request.CustomerEmail));
        for (var i = 0; i < request.Lines.Count; i++)
        {
            var line = request.Lines[i];
            form.Add(new($"line_items[{i}][quantity]", line.Quantity.ToString(CultureInfo.InvariantCulture)));
            form.Add(new($"line_items[{i}][price_data][currency]", "eur"));
            form.Add(new($"line_items[{i}][price_data][unit_amount]", line.UnitPriceCents.ToString(CultureInfo.InvariantCulture)));
            form.Add(new($"line_items[{i}][price_data][product_data][name]", line.Name));
        }

        try
        {
            using var message = Authorized(HttpMethod.Post, "checkout/sessions");
            message.Content = new FormUrlEncodedContent(form);
            message.Headers.Add("Idempotency-Key", request.OrderId.ToString());
            using var response = await http.SendAsync(message, ct);
            if (!response.IsSuccessStatusCode)
            {
                log.LogWarning("Stripe refused to open a payment page: HTTP {Status}", (int)response.StatusCode);
                return Error.BadGateway("Le paiement en ligne est indisponible, réessayez plus tard.");
            }

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
            var id = json.GetProperty("id").GetString();
            var url = json.GetProperty("url").GetString();
            return id is null || url is null
                ? Error.BadGateway("Le paiement en ligne est indisponible, réessayez plus tard.")
                : new PaymentSession(id, url);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException)
        {
            log.LogWarning(e, "Stripe is unreachable");
            return Error.BadGateway("Le paiement en ligne est indisponible, réessayez plus tard.");
        }
    }

    public async Task<PaymentState?> GetSessionStateAsync(string sessionId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Options.SecretKey))
            return null;

        try
        {
            using var message = Authorized(HttpMethod.Get, $"checkout/sessions/{Uri.EscapeDataString(sessionId)}");
            using var response = await http.SendAsync(message, ct);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadFromJsonAsync<JsonElement>(ct);
            return StateOf(json.GetProperty("status").GetString(), json.GetProperty("payment_status").GetString());
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException or JsonException or KeyNotFoundException)
        {
            log.LogWarning(e, "Stripe is unreachable");
            return null;
        }
    }

    public async Task CloseSessionAsync(string sessionId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(Options.SecretKey))
            return;

        try
        {
            using var message = Authorized(HttpMethod.Post, $"checkout/sessions/{Uri.EscapeDataString(sessionId)}/expire");
            message.Content = new FormUrlEncodedContent([]);
            using var response = await http.SendAsync(message, ct);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            log.LogWarning(e, "Stripe payment page could not be closed");
        }
    }

    public Result<PaymentNotification?> ReadNotification(string payload, string? signature)
    {
        if (!HasValidSignature(payload, signature))
            return Error.Validation("signature", "Signature invalide.");

        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;
            var type = root.GetProperty("type").GetString();
            var session = root.GetProperty("data").GetProperty("object");
            var id = session.GetProperty("id").GetString();
            if (id is null)
                return default(PaymentNotification);

            // checkout.session.completed with an unpaid status is a delayed payment method: the paid event comes later.
            PaymentNotification? notification = type switch
            {
                "checkout.session.completed" when session.GetProperty("payment_status").GetString() == "paid" => new(id, PaymentState.Paid),
                "checkout.session.async_payment_succeeded" => new(id, PaymentState.Paid),
                "checkout.session.expired" => new(id, PaymentState.Expired),
                _ => null,
            };
            return notification;
        }
        catch (Exception e) when (e is JsonException or KeyNotFoundException or InvalidOperationException)
        {
            return Error.Validation("payload", "Notification illisible.");
        }
    }

    private static PaymentState StateOf(string? status, string? paymentStatus) =>
        paymentStatus == "paid" ? PaymentState.Paid : status == "expired" ? PaymentState.Expired : PaymentState.Open;

    // Header "Stripe-Signature: t=<unix seconds>,v1=<hex HMAC-SHA256 of "<t>.<payload>">[,v1=...]".
    private bool HasValidSignature(string payload, string? header)
    {
        if (string.IsNullOrWhiteSpace(Options.WebhookSecret) || string.IsNullOrWhiteSpace(header))
            return false;

        var parts = header.Split(',').Select(p => p.Split('=', 2)).Where(p => p.Length == 2).ToList();
        var timestamp = parts.FirstOrDefault(p => p[0].Trim() == "t")?[1].Trim();
        if (!long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var seconds))
            return false;

        if ((clock.UtcNow - DateTimeOffset.FromUnixTimeSeconds(seconds)).Duration() > SignatureTolerance)
            return false;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Options.WebhookSecret));
        var expected = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{timestamp}.{payload}"))).ToLowerInvariant();
        return parts.Where(p => p[0].Trim() == "v1")
            .Any(p => CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(p[1].Trim())));
    }

    private HttpRequestMessage Authorized(HttpMethod method, string path)
    {
        var message = new HttpRequestMessage(method, new Uri(new Uri(Options.ApiBase), path));
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Options.SecretKey);
        return message;
    }
}
