using System.ComponentModel.DataAnnotations;
using InvoiceTracker.Invoicing.Domain.Invoices;

namespace InvoiceTracker.Invoicing.Api.Models.Request;

public class InvoiceDetailsRequest
{
    [Required]
    public required Guid? CustomerId
    {
        get; init;
    }

    [Range(0, Invoice.MaxPaymentTermsDays)]
    public int PaymentTermsDays
    {
        get; init;
    } = 30;

    [MaxLength(Invoice.MaxLines)]
    public IReadOnlyList<InvoiceLineRequest> Lines
    {
        get; init;
    } = [];
}
