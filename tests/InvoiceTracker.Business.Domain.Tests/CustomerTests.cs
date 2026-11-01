using InvoiceTracker.Business.Domain.Common;
using InvoiceTracker.Business.Domain.Customers;
using Shouldly;

namespace InvoiceTracker.Business.Domain.Tests;

public sealed class CustomerTests
{
    private static readonly Guid _businessId = Guid.CreateVersion7();
    private static readonly Guid _userId = Guid.CreateVersion7();
    private static readonly DateTime _now = new(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_SetsNameEmailAndAuditFields()
    {
        var customer = Customer.Create(_businessId, "  Dlamini Builders ", " accounts@dlamini.co.za ", _userId, _now);

        customer.BusinessId.ShouldBe(_businessId);
        customer.Name.ShouldBe("Dlamini Builders");
        customer.EmailAddress.Email.ShouldBe("accounts@dlamini.co.za");
        customer.CreatedBy.ShouldBe(_userId);
        customer.CreatedAt.ShouldBe(_now);
        customer.IsArchived.ShouldBeFalse();
    }

    [Fact]
    public void Create_WithBlankName_Throws()
    {
        Should.Throw<ArgumentException>(() => Customer.Create(_businessId, " ", "accounts@dlamini.co.za", _userId, _now));
    }

    [Fact]
    public void Create_WithoutBusinessId_Throws()
    {
        Should.Throw<ArgumentException>(() => Customer.Create(Guid.Empty, "Dlamini Builders", "accounts@dlamini.co.za", _userId, _now));
    }

    [Fact]
    public void UpdateDetails_TurnsBlankOptionalFieldsIntoNull()
    {
        var customer = NewCustomer();

        customer.UpdateDetails("Dlamini Builders (Pty) Ltd", "  ", "4123456789", _userId, _now.AddDays(1));

        customer.Name.ShouldBe("Dlamini Builders (Pty) Ltd");
        customer.CustomerReference.ShouldBeNull();
        customer.VatNumber.ShouldBe("4123456789");
        customer.ModifiedAt.ShouldBe(_now.AddDays(1));
    }

    [Fact]
    public void SetBillingAddress_Twice_UpdatesTheSameRow()
    {
        var customer = NewCustomer();
        customer.SetBillingAddress("1 Long Street", null, "Cape Town City Centre", "Cape Town", "8001", _userId, _now);
        var first = customer.BillingAddress;

        customer.SetBillingAddress("3 Long Street", null, "Cape Town City Centre", "Cape Town", "8001", _userId, _now);

        customer.BillingAddress.ShouldBeSameAs(first);
        customer.BillingAddress!.Line1.ShouldBe("3 Long Street");
    }

    [Fact]
    public void ClearBillingAddress_ReturnsTheRowToDelete()
    {
        var customer = NewCustomer();
        customer.SetBillingAddress("1 Long Street", null, "Cape Town City Centre", "Cape Town", "8001", _userId, _now);
        var address = customer.BillingAddress;

        var removed = customer.ClearBillingAddress(_userId, _now);

        removed.ShouldBeSameAs(address);
        customer.BillingAddress.ShouldBeNull();
        customer.BillingAddressId.ShouldBeNull();
    }

    [Fact]
    public void ClearContactNumber_WhenThereIsNone_ReturnsNull()
    {
        var customer = NewCustomer();

        customer.ClearContactNumber(_userId, _now).ShouldBeNull();
    }

    [Fact]
    public void Archive_Twice_KeepsItArchivedAndTheFirstTimestamp()
    {
        var customer = NewCustomer();

        customer.Archive(_userId, _now.AddDays(1));
        customer.Archive(_userId, _now.AddDays(2));

        customer.IsArchived.ShouldBeTrue();
        customer.ModifiedAt.ShouldBe(_now.AddDays(1));
    }

    [Fact]
    public void ChangingAnArchivedCustomer_Throws()
    {
        var customer = NewCustomer();
        customer.Archive(_userId, _now);

        Should.Throw<InvalidOperationException>(() => customer.UpdateDetails("New name", null, null, _userId, _now));
        Should.Throw<InvalidOperationException>(() => customer.SetEmail("new@dlamini.co.za", _userId, _now));
        Should.Throw<InvalidOperationException>(() => customer.SetContactNumber("+27821234567", ContactNumberType.Mobile, _userId, _now));
    }

    private static Customer NewCustomer()
    {
        return Customer.Create(_businessId, "Dlamini Builders", "accounts@dlamini.co.za", _userId, _now);
    }
}
