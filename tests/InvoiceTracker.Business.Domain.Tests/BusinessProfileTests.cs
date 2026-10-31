using InvoiceTracker.Business.Domain.Businesses;
using InvoiceTracker.Business.Domain.Common;
using Shouldly;

namespace InvoiceTracker.Business.Domain.Tests;

public sealed class BusinessProfileTests
{
    private static readonly Guid _businessId = Guid.CreateVersion7();
    private static readonly Guid _ownerId = Guid.CreateVersion7();
    private static readonly DateTime _now = new(2026, 10, 26, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_UsesTheBusinessIdFromIdentity()
    {
        var business = BusinessProfile.Create(_businessId, "  Mokoena Plumbing ", _ownerId, _now);

        business.Id.ShouldBe(_businessId);
        business.Name.ShouldBe("Mokoena Plumbing");
        business.CreatedBy.ShouldBe(_ownerId);
        business.DefaultPaymentTermsDays.ShouldBe(30);
        business.TimeZone.ShouldBe("Africa/Johannesburg");
    }

    [Fact]
    public void Create_WithoutBusinessId_Throws()
    {
        Should.Throw<ArgumentException>(() => BusinessProfile.Create(Guid.Empty, "Mokoena Plumbing", _ownerId, _now));
    }

    [Fact]
    public void SetAddress_Twice_UpdatesTheSameRow()
    {
        var business = BusinessProfile.Create(_businessId, "Mokoena Plumbing", _ownerId, _now);
        business.SetAddress("12 Jan Smuts Avenue", null, "Parktown", "Johannesburg", "2193", _ownerId, _now);
        var first = business.Address;

        business.SetAddress("14 Jan Smuts Avenue", "Unit 3", "Parktown", "Johannesburg", "2193", _ownerId, _now.AddDays(1));

        business.Address.ShouldBeSameAs(first);
        business.Address!.Line1.ShouldBe("14 Jan Smuts Avenue");
        business.Address.ModifiedAt.ShouldBe(_now.AddDays(1));
    }

    [Fact]
    public void ClearContactNumber_ReturnsTheRowToDelete()
    {
        var business = BusinessProfile.Create(_businessId, "Mokoena Plumbing", _ownerId, _now);
        business.SetContactNumber("+27821234567", ContactNumberType.Mobile, _ownerId, _now);
        var number = business.ContactNumber;

        var removed = business.ClearContactNumber(_ownerId, _now);

        removed.ShouldBeSameAs(number);
        business.ContactNumber.ShouldBeNull();
        business.ContactNumberId.ShouldBeNull();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(BusinessProfile.MaxPaymentTermsDays + 1)]
    public void UpdateSettings_PaymentTermsOutOfRange_Throws(int days)
    {
        var business = BusinessProfile.Create(_businessId, "Mokoena Plumbing", _ownerId, _now);

        Should.Throw<ArgumentOutOfRangeException>(() => business.UpdateSettings(days, "Africa/Johannesburg", _ownerId, _now));
    }

    [Fact]
    public void UpdateDetails_StampsModifiedAndBlanksBecomeNull()
    {
        var business = BusinessProfile.Create(_businessId, "Mokoena Plumbing", _ownerId, _now);

        business.UpdateDetails("Mokoena Plumbing & Gas", " ", "4123456789", _ownerId, _now.AddHours(1));

        business.RegistrationNumber.ShouldBeNull();
        business.VatNumber.ShouldBe("4123456789");
        business.ModifiedAt.ShouldBe(_now.AddHours(1));
        business.ModifiedBy.ShouldBe(_ownerId);
    }

    [Fact]
    public void ReplaceLogo_ReturnsThePreviousPath()
    {
        var business = BusinessProfile.Create(_businessId, "Mokoena Plumbing", _ownerId, _now);
        business.ReplaceLogo("a/old.png", _ownerId, _now);

        var previous = business.ReplaceLogo("a/new.png", _ownerId, _now);

        previous.ShouldBe("a/old.png");
        business.LogoBlobPath.ShouldBe("a/new.png");
    }
}
