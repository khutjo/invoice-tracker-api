namespace InvoiceTracker.BuildingBlocks.ServiceDefaults.Authentication;

/// <summary>Values of the role claim; they match Identity's UserRole names.</summary>
public static class InvoiceTrackerRoles
{
    public const string Owner = "Owner";
    public const string Accountant = "Accountant";
}
