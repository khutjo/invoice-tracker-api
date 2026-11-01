using InvoiceTracker.Invoicing.Domain.Invoices;
using Shouldly;

namespace InvoiceTracker.Invoicing.Domain.Tests;

public sealed class InvoiceTests
{
    private static readonly Guid _businessId = Guid.CreateVersion7();
    private static readonly Guid _customerId = Guid.CreateVersion7();
    private static readonly Guid _userId = Guid.CreateVersion7();
    private static readonly DateTime _now = new(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly _today = new(2026, 11, 1);
    private static readonly BilledCustomer _customer =
        BilledCustomer.Create("Dlamini Builders", "accounts@dlamini.co.za", "4123456789", "1 Long Street", null, "Cape Town City Centre", "Cape Town", "8001");

    [Fact]
    public void CreateDraft_WorksOutTotalsAndRecordsCreated()
    {
        var invoice = Invoice.CreateDraft(_businessId, _customerId, 30,
        [
            new InvoiceLineDetails("Call-out fee", 1, 450.00m, 0.15m),
            new InvoiceLineDetails("Labour (hours)", 2.5m, 380.00m, 0.15m)
        ], _userId, _now);

        invoice.Status.ShouldBe(InvoiceStatus.Draft);
        invoice.InvoiceNumber.ShouldBeNull();
        invoice.Subtotal.ShouldBe(1400.00m);
        invoice.VatTotal.ShouldBe(210.00m);
        invoice.Total.ShouldBe(1610.00m);
        invoice.Balance.ShouldBe(1610.00m);
        invoice.Lines.Select(l => l.LineNumber).ToArray().ShouldBe(new[] { 1, 2 });
        invoice.Events.ShouldHaveSingleItem().EventType.ShouldBe(InvoiceEventType.Created);
    }

    [Fact]
    public void Vat_IsRoundedPerLine_NotOnTheSum()
    {
        // Each line: 0.03 x 15% = 0.0045, which rounds to 0.00. Rounding the sum (0.009) would give 0.01.
        var invoice = Invoice.CreateDraft(_businessId, _customerId, 30,
        [
            new InvoiceLineDetails("Washer", 1, 0.03m, 0.15m),
            new InvoiceLineDetails("Washer", 1, 0.03m, 0.15m)
        ], _userId, _now);

        invoice.Lines.Select(l => l.VatAmount).ToArray().ShouldBe(new[] { 0.00m, 0.00m });
        invoice.VatTotal.ShouldBe(0.00m);
        invoice.Total.ShouldBe(0.06m);
    }

    [Fact]
    public void LineTotal_HalfACent_RoundsUp()
    {
        var invoice = Invoice.CreateDraft(_businessId, _customerId, 30,
        [
            new InvoiceLineDetails("Half a cent", 0.5m, 0.01m, 0m)
        ], _userId, _now);

        invoice.Lines.ShouldHaveSingleItem().LineTotal.ShouldBe(0.01m);
    }

    [Fact]
    public void CreateDraft_WithoutCustomer_Throws()
    {
        Should.Throw<ArgumentException>(() => Invoice.CreateDraft(_businessId, Guid.Empty, 30, [], _userId, _now));
    }

    [Fact]
    public void CreateDraft_WithZeroQuantity_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() =>
            Invoice.CreateDraft(_businessId, _customerId, 30, [new InvoiceLineDetails("Nothing", 0, 10m, 0.15m)], _userId, _now));
    }

    [Fact]
    public void UpdateDraft_ReplacesLinesAndRecalculates()
    {
        var invoice = NewDraft();

        invoice.UpdateDraft(_customerId, 14, [new InvoiceLineDetails("Geyser element", 1, 1299.99m, 0.15m)], _userId, _now.AddHours(1));

        invoice.Lines.ShouldHaveSingleItem().Description.ShouldBe("Geyser element");
        invoice.PaymentTermsDays.ShouldBe(14);
        invoice.Subtotal.ShouldBe(1299.99m);
        invoice.VatTotal.ShouldBe(195.00m);
        invoice.Total.ShouldBe(1494.99m);
        invoice.ModifiedAt.ShouldBe(_now.AddHours(1));
        invoice.Events.Last().EventType.ShouldBe(InvoiceEventType.Edited);
    }

    [Fact]
    public void Send_SetsNumberDatesAndStatus()
    {
        var invoice = NewDraft();

        invoice.Send("INV-2026-0001", _today, _customer, _userId, _now);

        invoice.Status.ShouldBe(InvoiceStatus.Sent);
        invoice.InvoiceNumber.ShouldBe("INV-2026-0001");
        invoice.IssueDate.ShouldBe(_today);
        invoice.DueDate.ShouldBe(new DateOnly(2026, 12, 1));
        invoice.Events.Last().EventType.ShouldBe(InvoiceEventType.Sent);
    }

    [Fact]
    public void Send_FreezesTheCustomerDetails()
    {
        var invoice = NewDraft();
        invoice.BilledCustomer.ShouldBeNull();

        invoice.Send("INV-2026-0001", _today, _customer, _userId, _now);

        invoice.BilledCustomer.ShouldBeSameAs(_customer);
        invoice.BilledCustomer!.Name.ShouldBe("Dlamini Builders");
    }

    [Fact]
    public void Send_WithoutCustomerDetails_Throws()
    {
        var invoice = NewDraft();

        Should.Throw<ArgumentNullException>(() => invoice.Send("INV-2026-0001", _today, null!, _userId, _now));
        invoice.Status.ShouldBe(InvoiceStatus.Draft);
    }

    [Fact]
    public void BilledCustomer_TrimsAndTurnsBlankOptionalFieldsIntoNull()
    {
        var customer = BilledCustomer.Create(" Dlamini Builders ", " accounts@dlamini.co.za ", " ", null, null, "", null, null);

        customer.Name.ShouldBe("Dlamini Builders");
        customer.Email.ShouldBe("accounts@dlamini.co.za");
        customer.VatNumber.ShouldBeNull();
        customer.Suburb.ShouldBeNull();
    }

    [Fact]
    public void Send_WithoutLines_IsRefused()
    {
        var invoice = Invoice.CreateDraft(_businessId, _customerId, 30, [], _userId, _now);

        Should.Throw<InvoiceRuleException>(() => invoice.Send("INV-2026-0001", _today, _customer, _userId, _now));
        invoice.Status.ShouldBe(InvoiceStatus.Draft);
    }

    [Fact]
    public void Send_WithZeroTotal_IsRefused()
    {
        var invoice = Invoice.CreateDraft(_businessId, _customerId, 30, [new InvoiceLineDetails("Free", 1, 0m, 0.15m)], _userId, _now);

        Should.Throw<InvoiceRuleException>(() => invoice.Send("INV-2026-0001", _today, _customer, _userId, _now));
    }

    [Fact]
    public void SentInvoice_CannotBeSentEditedOrDeleted()
    {
        var invoice = NewDraft();
        invoice.Send("INV-2026-0001", _today, _customer, _userId, _now);

        Should.Throw<InvoiceRuleException>(() => invoice.Send("INV-2026-0002", _today, _customer, _userId, _now));
        Should.Throw<InvoiceRuleException>(() => invoice.UpdateDraft(_customerId, 30, [new InvoiceLineDetails("X", 1, 1m, 0m)], _userId, _now));
        Should.Throw<InvoiceRuleException>(() => invoice.EnsureCanDelete());
        invoice.InvoiceNumber.ShouldBe("INV-2026-0001");
    }

    [Fact]
    public void Draft_CanBeDeleted()
    {
        var invoice = NewDraft();

        Should.NotThrow(() => invoice.EnsureCanDelete());
    }

    private static Invoice NewDraft()
    {
        return Invoice.CreateDraft(_businessId, _customerId, 30, [new InvoiceLineDetails("Call-out fee", 1, 450.00m, 0.15m)], _userId, _now);
    }
}
