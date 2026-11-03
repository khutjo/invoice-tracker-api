using System.Globalization;
using InvoiceTracker.Invoicing.Domain.Invoices;
using Shouldly;

namespace InvoiceTracker.Invoicing.Domain.Tests;

public sealed class InvoicePaymentTests
{
    private static readonly DateTime _now = new(2026, 11, 5, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly _paidOn = new(2026, 11, 5);

    [Fact]
    public void PartialPayment_ReducesTheBalanceAndStaysSent()
    {
        var invoice = InvoiceOverdueTests.NewSentInvoice();

        invoice.RecordPayment(610.00m, _paidOn, PaymentMethod.Eft, "INV-2026-0001", "evt_1", null, _now);

        invoice.AmountPaid.ShouldBe(610.00m);
        invoice.Balance.ShouldBe(1000.00m);
        invoice.Status.ShouldBe(InvoiceStatus.Sent);
        invoice.Payments.ShouldHaveSingleItem().ExternalId.ShouldBe("evt_1");
        invoice.Events.Last().EventType.ShouldBe(InvoiceEventType.PaymentRecorded);
    }

    [Fact]
    public void PaymentEqualToTheBalance_MovesToPaid()
    {
        var invoice = InvoiceOverdueTests.NewSentInvoice();

        invoice.RecordPayment(1610.00m, _paidOn, PaymentMethod.Eft, null, null, null, _now);

        invoice.Balance.ShouldBe(0m);
        invoice.Status.ShouldBe(InvoiceStatus.Paid);
        invoice.Events.Select(e => e.EventType).TakeLast(2).ToArray().ShouldBe(new[] { InvoiceEventType.PaymentRecorded, InvoiceEventType.Paid });
    }

    [Fact]
    public void TwoPartialPayments_ThatCoverTheBalance_MoveToPaid()
    {
        var invoice = InvoiceOverdueTests.NewSentInvoice();

        invoice.RecordPayment(1000.00m, _paidOn, PaymentMethod.Eft, null, null, null, _now);
        invoice.RecordPayment(610.00m, _paidOn, PaymentMethod.Card, null, null, null, _now);

        invoice.Status.ShouldBe(InvoiceStatus.Paid);
        invoice.Payments.Count.ShouldBe(2);
    }

    [Fact]
    public void Overpayment_MovesToPaidAndLeavesACredit()
    {
        var invoice = InvoiceOverdueTests.NewSentInvoice();

        invoice.RecordPayment(1700.00m, _paidOn, PaymentMethod.Eft, null, null, null, _now);

        invoice.AmountPaid.ShouldBe(1700.00m);
        invoice.Balance.ShouldBe(-90.00m);
        invoice.Status.ShouldBe(InvoiceStatus.Paid);
        invoice.Events.Last().EventType.ShouldBe(InvoiceEventType.Paid);
    }

    [Fact]
    public void PartialThenOverpayment_MovesToPaid()
    {
        var invoice = InvoiceOverdueTests.NewSentInvoice();

        invoice.RecordPayment(1000.00m, _paidOn, PaymentMethod.Eft, null, null, null, _now);
        invoice.RecordPayment(610.01m, _paidOn, PaymentMethod.Eft, null, null, null, _now);

        invoice.Balance.ShouldBe(-0.01m);
        invoice.Status.ShouldBe(InvoiceStatus.Paid);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-10")]
    [InlineData("10.005")]
    public void ZeroNegativeOrFractionalCentPayments_AreRefused(string amountText)
    {
        var amount = decimal.Parse(amountText, CultureInfo.InvariantCulture);
        var invoice = InvoiceOverdueTests.NewSentInvoice();

        Should.Throw<InvoiceRuleException>(() =>
            invoice.RecordPayment(amount, _paidOn, PaymentMethod.Eft, null, null, null, _now));
    }

    [Fact]
    public void PaymentOnADraft_IsRefused()
    {
        var invoice = InvoiceOverdueTests.NewDraft();

        Should.Throw<InvoiceRuleException>(() =>
            invoice.RecordPayment(100m, _paidOn, PaymentMethod.Eft, null, null, null, _now));
    }

    [Fact]
    public void PaymentOnAPaidInvoice_IsRefused()
    {
        var invoice = InvoiceOverdueTests.NewSentInvoice();
        invoice.RecordPayment(1610.00m, _paidOn, PaymentMethod.Eft, null, null, null, _now);

        Should.Throw<InvoiceRuleException>(() =>
            invoice.RecordPayment(1m, _paidOn, PaymentMethod.Eft, null, null, null, _now));
    }

    [Fact]
    public void OverdueInvoice_PaidInFull_MovesToPaid()
    {
        var invoice = InvoiceOverdueTests.NewSentInvoice();
        invoice.CheckOverdue(new DateOnly(2026, 12, 5), _now);
        invoice.Status.ShouldBe(InvoiceStatus.Overdue);

        invoice.RecordPayment(invoice.Balance, new DateOnly(2026, 12, 5), PaymentMethod.Eft, null, null, null, _now);

        invoice.Status.ShouldBe(InvoiceStatus.Paid);
    }
}
