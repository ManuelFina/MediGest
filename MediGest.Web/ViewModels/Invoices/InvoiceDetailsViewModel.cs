
using MediGest.Web.Models.Enums;

namespace MediGest.Web.ViewModels.Invoices;

public class InvoiceDetailsViewModel
{
    public int Id { get; set; }

    public string Number { get; set; } = string.Empty;

    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string PatientDocumentNumber { get; set; } = string.Empty;

    public string? PatientPhoneNumber { get; set; }

    public string? HealthInsuranceName { get; set; }

    public string? HealthPlanName { get; set; }

    public string? MembershipNumber { get; set; }

    public DateOnly IssueDate { get; set; }

    public string? Notes { get; set; }

    public decimal Subtotal { get; set; }

    public decimal CoverageAmount { get; set; }

    public decimal PatientAmount { get; set; }

    public decimal Total { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal PendingAmount { get; set; }

    public InvoiceStatus Status { get; set; }

    public List<InvoiceDetailItemViewModel> Details { get; set; } = new();

    public List<InvoicePaymentItemViewModel> Payments { get; set; } = new();
}

public class InvoiceDetailItemViewModel
{
    public string Description { get; set; } = string.Empty;

    public DateOnly ServiceDate { get; set; }

    public string? AuthorizationNumber { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal UnitCoverageAmount { get; set; }

    public decimal Subtotal { get; set; }

    public decimal CoverageAmount { get; set; }

    public decimal PatientAmount { get; set; }

    public int? AppointmentId { get; set; }
}

public class InvoicePaymentItemViewModel
{
    public int Id { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod Method { get; set; }

    public string? Reference { get; set; }

    public string? Notes { get; set; }
}
