
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.ViewModels.HealthPlanTariffs;

public class HealthPlanTariffFormViewModel
{
    [Required(ErrorMessage = "El plan de cobertura es obligatorio.")]
    [Display(Name = "Plan de cobertura")]
    public int HealthPlanId { get; set; }

    [Required(ErrorMessage = "La prestación médica es obligatoria.")]
    [Display(Name = "Prestación médica")]
    public int MedicalServiceId { get; set; }

    [Required(ErrorMessage = "El importe reconocido es obligatorio.")]
    [Range(0.01, 999999999999.99,
        ErrorMessage = "El importe debe ser mayor a cero.")]
    [Display(Name = "Importe reconocido")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "La fecha de inicio de vigencia es obligatoria.")]
    [Display(Name = "Vigente desde")]
    public DateOnly ValidFrom { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Vigente hasta")]
    public DateOnly? ValidTo { get; set; }

    [Display(Name = "Requiere autorización")]
    public bool RequiresAuthorization { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    [Display(Name = "Observaciones")]
    public string? Notes { get; set; }

    public IEnumerable<SelectListItem> HealthPlans { get; set; }
        = new List<SelectListItem>();

    public IEnumerable<SelectListItem> MedicalServices { get; set; }
        = new List<SelectListItem>();
}
