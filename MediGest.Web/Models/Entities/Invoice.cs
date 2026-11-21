
using System.ComponentModel.DataAnnotations;
using MediGest.Web.Models.Enums;

namespace MediGest.Web.Models.Entities;

public class Invoice
{
    public int Id { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Número de comprobante")]
    public string Number { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Paciente")]
    public int PatientId { get; set; }

    public Patient Patient { get; set; } = null!;

    [Display(Name = "Afiliación")]
    public int? PatientAffiliationId { get; set; }

    public PatientAffiliation? PatientAffiliation { get; set; }

    [Required]
    [Display(Name = "Fecha de emisión")]
    public DateOnly IssueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Required]
    [Range(0, 999999999999.99)]
    [Display(Name = "Subtotal")]
    public decimal Subtotal { get; set; }

    [Required]
    [Range(0, 999999999999.99)]
    [Display(Name = "Importe cubierto")]
    public decimal CoverageAmount { get; set; }

    [Required]
    [Range(0, 999999999999.99)]
    [Display(Name = "Total a cargo del paciente")]
    public decimal PatientAmount { get; set; }

    [Required]
    [Range(0, 999999999999.99)]
    [Display(Name = "Total")]
    public decimal Total { get; set; }

    [Required]
    public InvoiceStatus Status { get; set; }
        = InvoiceStatus.Pending;

    [StringLength(500)]
    [Display(Name = "Observaciones")]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<InvoiceDetail> Details { get; set; }
        = new List<InvoiceDetail>();

    public ICollection<Payment> Payments { get; set; }
        = new List<Payment>();
}
