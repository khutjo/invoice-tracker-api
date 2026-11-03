using InvoiceTracker.Invoicing.Domain.Invoices;
using Shouldly;

namespace InvoiceTracker.Invoicing.Domain.Tests;

public sealed class InvoiceOverdueTests
{
    private static readonly DateTime _now = new(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc);

    // Issued 1 Nov with 30-day terms, so due 1 Dec
    private static readonly DateOnly _issueDate = new(2026, 11, 1);
    private static readonly DateOnly _dueDate = new(2026, 12, 1);

    [Fact]
    public void DueToday_IsNotOverdue()
    {
        var invoice = NewSentInvoice();

        invoice.CheckOverdue(_dueDate, _now).ShouldBeNull();

        invoice.Status.ShouldBe(InvoiceStatus.Sent);
        invoice.ReminderStage.ShouldBe(OverdueReminderStage.None);
    }

    [Fact]
    public void DayAfterDueDate_MarksOverdueAndRaisesFirstReminder()
    {
        var invoice = NewSentInvoice();

        var stage = invoice.CheckOverdue(_dueDate.AddDays(1), _now);

        stage.ShouldBe(OverdueReminderStage.First);
        invoice.Status.ShouldBe(InvoiceStatus.Overdue);
        invoice.ReminderStage.ShouldBe(OverdueReminderStage.First);
        invoice.Events.Select(e => e.EventType).TakeLast(2).ToArray().ShouldBe(new[] { InvoiceEventType.MarkedOverdue, InvoiceEventType.ReminderRaised });
    }

    [Fact]
    public void ScanningTwiceOnTheSameDay_RaisesOneReminder()
    {
        var invoice = NewSentInvoice();
        var today = _dueDate.AddDays(1);

        invoice.CheckOverdue(today, _now).ShouldBe(OverdueReminderStage.First);
        invoice.CheckOverdue(today, _now.AddMinutes(1)).ShouldBeNull();

        invoice.Events.Count(e => e.EventType == InvoiceEventType.ReminderRaised).ShouldBe(1);
    }

    [Theory]
    [InlineData(6, OverdueReminderStage.First)]
    [InlineData(7, OverdueReminderStage.Second)]
    [InlineData(13, OverdueReminderStage.Second)]
    [InlineData(14, OverdueReminderStage.Final)]
    [InlineData(60, OverdueReminderStage.Final)]
    public void ReminderStage_EscalatesAt7And14Days(int daysOverdue, OverdueReminderStage expected)
    {
        Invoice.ReminderStageFor(daysOverdue).ShouldBe(expected);
    }

    [Fact]
    public void Escalation_RaisesSecondThenFinal_EachOnce()
    {
        var invoice = NewSentInvoice();

        invoice.CheckOverdue(_dueDate.AddDays(1), _now).ShouldBe(OverdueReminderStage.First);
        invoice.CheckOverdue(_dueDate.AddDays(6), _now).ShouldBeNull();
        invoice.CheckOverdue(_dueDate.AddDays(7), _now).ShouldBe(OverdueReminderStage.Second);
        invoice.CheckOverdue(_dueDate.AddDays(10), _now).ShouldBeNull();
        invoice.CheckOverdue(_dueDate.AddDays(14), _now).ShouldBe(OverdueReminderStage.Final);
        invoice.CheckOverdue(_dueDate.AddDays(30), _now).ShouldBeNull();
    }

    [Fact]
    public void ScannerThatMissedDays_JumpsStraightToTheStageDueNow()
    {
        var invoice = NewSentInvoice();

        invoice.CheckOverdue(_dueDate.AddDays(20), _now).ShouldBe(OverdueReminderStage.Final);

        invoice.Events.Count(e => e.EventType == InvoiceEventType.ReminderRaised).ShouldBe(1);
    }

    [Fact]
    public void Draft_IsNeverOverdue()
    {
        var invoice = NewDraft();

        invoice.CheckOverdue(_dueDate.AddDays(30), _now).ShouldBeNull();
        invoice.Status.ShouldBe(InvoiceStatus.Draft);
    }

    [Fact]
    public void PaidInvoice_IsNeverOverdue()
    {
        var invoice = NewSentInvoice();
        invoice.RecordPayment(invoice.Balance, _issueDate, PaymentMethod.Eft, null, null, null, _now);

        invoice.CheckOverdue(_dueDate.AddDays(30), _now).ShouldBeNull();
        invoice.Status.ShouldBe(InvoiceStatus.Paid);
    }

    [Fact]
    public void DaysOverdue_CountsWholeDaysPastTheDueDate()
    {
        var invoice = NewSentInvoice();

        invoice.DaysOverdue(_dueDate).ShouldBe(0);
        invoice.DaysOverdue(_dueDate.AddDays(9)).ShouldBe(9);
    }

    internal static Invoice NewDraft()
    {
        return Invoice.CreateDraft(Guid.CreateVersion7(), Guid.CreateVersion7(), 30,
        [
            new InvoiceLineDetails("Call-out fee", 1, 450.00m, 0.15m),
            new InvoiceLineDetails("Labour (hours)", 2.5m, 380.00m, 0.15m)
        ], Guid.CreateVersion7(), _now);
    }

    internal static Invoice NewSentInvoice()
    {
        var invoice = NewDraft();
        var customer = BilledCustomer.Create("Dlamini Builders", "accounts@dlamini.co.za", null, null, null, null, null, null);
        invoice.Send("INV-2026-0001", _issueDate, customer, Guid.CreateVersion7(), _now);
        return invoice;
    }
}
