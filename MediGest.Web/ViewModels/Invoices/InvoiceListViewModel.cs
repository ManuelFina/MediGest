
using MediGest.Web.Models.Enums;

namespace MediGest.Web.ViewModels.Invoices;

public class InvoiceListViewModel
{
    public IEnumerable<InvoiceListItemViewModel> Invoices { get; set; } =
        Enumerable.Empty<InvoiceListItemViewModel>();

    public int TotalInvoices { get; set; }

    public int PendingInvoices { get; set; }

    public int PartiallyPaidInvoices { get; set; }

    public int PaidInvoices { get; set; }

    public decimal TotalBilled { get; set; }

    public decimal TotalPending { get; set; }
}

public class InvoiceListItemViewModel
{
    public int Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public string PatientName { get; set; } = string.Empty;

    public string? HealthPlanName { get; set; }

    public DateOnly IssueDate { get; set; }

    public decimal Total { get; set; }

    public decimal CoverageAmount { get; set; }

    public decimal PatientAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal PendingAmount { get; set; }

    public InvoiceStatus Status { get; set; }
}
