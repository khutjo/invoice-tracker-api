using InvoiceTracker.Invoicing.Domain.Invoices;
using Shouldly;

namespace InvoiceTracker.Invoicing.Domain.Tests;

public sealed class InvoiceNumberSequenceTests
{
    [Fact]
    public void Next_CountsUpFromOne_WithFourDigits()
    {
        var sequence = InvoiceNumberSequence.Start(Guid.CreateVersion7(), 2026);

        sequence.Next().ShouldBe("INV-2026-0001");
        sequence.Next().ShouldBe("INV-2026-0002");
        sequence.LastNumber.ShouldBe(2);
    }

    [Fact]
    public void Format_GrowsPastFourDigits()
    {
        InvoiceNumberSequence.Format(2026, 12345).ShouldBe("INV-2026-12345");
    }

    [Fact]
    public void Start_WithoutBusiness_Throws()
    {
        Should.Throw<ArgumentException>(() => InvoiceNumberSequence.Start(Guid.Empty, 2026));
    }
}
