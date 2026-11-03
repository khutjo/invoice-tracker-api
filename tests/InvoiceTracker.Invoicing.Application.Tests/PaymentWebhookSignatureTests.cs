using InvoiceTracker.Invoicing.Application.Payments;
using Shouldly;

namespace InvoiceTracker.Invoicing.Application.Tests;

public sealed class PaymentWebhookSignatureTests
{
    private const string Secret = "test-webhook-secret-with-enough-length";
    private const string Body = """{"eventId":"evt_1","eventType":"payment.succeeded","amount":100.00}""";
    private static readonly DateTimeOffset _now = new(2026, 11, 3, 10, 0, 0, TimeSpan.Zero);
    private static readonly TimeSpan _tolerance = TimeSpan.FromMinutes(5);
    private static readonly string _timestamp = _now.ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);

    [Fact]
    public void SignatureFromTheSameSecretAndBody_IsValid()
    {
        var signature = PaymentWebhookSignature.Compute(Secret, _timestamp, Body);

        signature.ShouldStartWith("v1=");
        PaymentWebhookSignature.IsValid(Secret, _timestamp, signature, Body, _now, _tolerance).ShouldBeTrue();
    }

    [Fact]
    public void TamperedBody_IsRejected()
    {
        var signature = PaymentWebhookSignature.Compute(Secret, _timestamp, Body);

        PaymentWebhookSignature.IsValid(Secret, _timestamp, signature, Body.Replace("100.00", "999.00"), _now, _tolerance).ShouldBeFalse();
    }

    [Fact]
    public void WrongSecret_IsRejected()
    {
        var signature = PaymentWebhookSignature.Compute("another-secret-of-a-decent-length-0000", _timestamp, Body);

        PaymentWebhookSignature.IsValid(Secret, _timestamp, signature, Body, _now, _tolerance).ShouldBeFalse();
    }

    [Fact]
    public void TimestampOlderThanTheTolerance_IsRejected()
    {
        var signature = PaymentWebhookSignature.Compute(Secret, _timestamp, Body);

        PaymentWebhookSignature.IsValid(Secret, _timestamp, signature, Body, _now.AddMinutes(6), _tolerance).ShouldBeFalse();
    }

    [Theory]
    [InlineData(null, "v1=abc")]
    [InlineData("not-a-number", "v1=abc")]
    [InlineData("1793700000", null)]
    public void MissingOrMalformedHeaders_AreRejected(string? timestamp, string? signature)
    {
        PaymentWebhookSignature.IsValid(Secret, timestamp, signature, Body, _now, _tolerance).ShouldBeFalse();
    }
}
