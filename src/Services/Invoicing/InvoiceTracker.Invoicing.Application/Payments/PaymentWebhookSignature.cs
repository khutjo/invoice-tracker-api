using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace InvoiceTracker.Invoicing.Application.Payments;

/// <summary>
/// Verifies the payment processor's webhook signature. The processor signs "{timestamp}.{raw body}" with
/// HMAC-SHA256 and the shared secret, and sends <c>Webhook-Timestamp</c> (Unix seconds) and
/// <c>Webhook-Signature: v1=&lt;hex&gt;</c>. The timestamp is signed too, so an old request can't be replayed later.
/// </summary>
public static class PaymentWebhookSignature
{
    public const string Scheme = "v1=";

    public static string Compute(string secret, string timestamp, string body)
    {
        ArgumentException.ThrowIfNullOrEmpty(secret);

        var key = Encoding.UTF8.GetBytes(secret);
        var payload = Encoding.UTF8.GetBytes($"{timestamp}.{body}");

        return Scheme + Convert.ToHexStringLower(HMACSHA256.HashData(key, payload));
    }

    /// <summary>True when the signature matches and the timestamp is within <paramref name="tolerance"/> of now.</summary>
    public static bool IsValid(string secret, string? timestamp, string? signature, string body, DateTimeOffset now, TimeSpan tolerance)
    {
        if (string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(signature))
        {
            return false;
        }

        if (!long.TryParse(timestamp, NumberStyles.None, CultureInfo.InvariantCulture, out var unixSeconds))
        {
            return false;
        }

        var sentAt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
        if ((now - sentAt).Duration() > tolerance)
        {
            return false;
        }

        var expected = Encoding.ASCII.GetBytes(Compute(secret, timestamp, body));
        var actual = Encoding.ASCII.GetBytes(signature.Trim().ToLowerInvariant());

        // Constant time, so response timing reveals nothing about how much of a guess was right
        return CryptographicOperations.FixedTimeEquals(expected, actual);
    }
}
