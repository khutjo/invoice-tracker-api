using InvoiceTracker.Business.Domain.Businesses;
using InvoiceTracker.Business.Domain.Common;
using Shouldly;

namespace InvoiceTracker.Business.Domain.Tests;

public sealed class BusinessProfileTests
{
    private static readonly Guid BusinessId = Guid.CreateVersion7();
    private static readonly Guid OwnerId = Guid.CreateVersion7();
    private static readonly DateTime Now = new(2026, 10, 26, 8, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Create_UsesTheBusinessIdFromIdentity()
    {
        var business = BusinessProfile.Create(BusinessId, "  Mokoena Plumbing ", OwnerId, Now);

        business.Id.ShouldBe(BusinessId);
        business.Name.ShouldBe("Mokoena Plumbing");
        business.CreatedBy.ShouldBe(OwnerId);
        business.DefaultPaymentTermsDays.ShouldBe(30);
        business.TimeZone.ShouldBe("Africa/Johannesburg");
    }

    [Fact]
    public void Create_WithoutBusinessId_Throws()
    {
        Should.Throw<ArgumentException>(() => BusinessProfile.Create(Guid.Empty, "Mokoena Plumbing", OwnerId, Now));
    }

    [Fact]
    public void SetAddress_Twice_UpdatesTheSameRow()
    {
        var business = BusinessProfile.Create(BusinessId, "Mokoena Plumbing", OwnerId, Now);
        business.SetAddress("12 Jan Smuts Avenue", null, "Parktown", "Johannesburg", "2193", OwnerId, Now);
        var first = business.Address;

        business.SetAddress("14 Jan Smuts Avenue", "Unit 3", "Parktown", "Johannesburg", "2193", OwnerId, Now.AddDays(1));

        business.Address.ShouldBeSameAs(first);
        business.Address!.Line1.ShouldBe("14 Jan Smuts Avenue");
        business.Address.ModifiedAt.ShouldBe(Now.AddDays(1));
    }

    [Fact]
    public void ClearContactNumber_ReturnsTheRowToDelete()
    {
        var business = BusinessProfile.Create(BusinessId, "Mokoena Plumbing", OwnerId, Now);
        business.SetContactNumber("+27821234567", ContactNumberType.Mobile, OwnerId, Now);
        var number = business.ContactNumber;

        var removed = business.ClearContactNumber(OwnerId, Now);

        removed.ShouldBeSameAs(number);
        business.ContactNumber.ShouldBeNull();
        business.ContactNumberId.ShouldBeNull();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(BusinessProfile.MaxPaymentTermsDays + 1)]
    public void UpdateSettings_PaymentTermsOutOfRange_Throws(int days)
    {
        var business = BusinessProfile.Create(BusinessId, "Mokoena Plumbing", OwnerId, Now);

        Should.Throw<ArgumentOutOfRangeException>(() => business.UpdateSettings(days, "Africa/Johannesburg", OwnerId, Now));
    }

    [Fact]
    public void UpdateDetails_StampsModifiedAndBlanksBecomeNull()
    {
        var business = BusinessProfile.Create(BusinessId, "Mokoena Plumbing", OwnerId, Now);

        business.UpdateDetails("Mokoena Plumbing & Gas", " ", "4123456789", OwnerId, Now.AddHours(1));

        business.RegistrationNumber.ShouldBeNull();
        business.VatNumber.ShouldBe("4123456789");
        business.ModifiedAt.ShouldBe(Now.AddHours(1));
        business.ModifiedBy.ShouldBe(OwnerId);
    }

    [Fact]
    public void ReplaceLogo_ReturnsThePreviousPath()
    {
        var business = BusinessProfile.Create(BusinessId, "Mokoena Plumbing", OwnerId, Now);
        business.ReplaceLogo("a/old.png", OwnerId, Now);

        var previous = business.ReplaceLogo("a/new.png", OwnerId, Now);

        previous.ShouldBe("a/old.png");
        business.LogoBlobPath.ShouldBe("a/new.png");
    }
}
