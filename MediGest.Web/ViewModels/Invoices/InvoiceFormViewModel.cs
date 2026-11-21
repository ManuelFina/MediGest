
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.ViewModels.Invoices;

public class InvoiceFormViewModel
{
    [Required(ErrorMessage = "El paciente es obligatorio.")]
    [Display(Name = "Paciente")]
    public int PatientId { get; set; }

    [Display(Name = "Afiliación")]
    public int? PatientAffiliationId { get; set; }

    [Required(ErrorMessage = "La fecha de emisión es obligatoria.")]
    [Display(Name = "Fecha de emisión")]
    public DateOnly IssueDate { get; set; } =
        DateOnly.FromDateTime(DateTime.Today);

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    [Display(Name = "Observaciones")]
    public string? Notes { get; set; }

    public List<InvoiceDetailFormViewModel> Details { get; set; } = new();

    public IEnumerable<SelectListItem> Patients { get; set; } =
        Enumerable.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> Affiliations { get; set; } =
        Enumerable.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> MedicalServices { get; set; } =
        Enumerable.Empty<SelectListItem>();
}
