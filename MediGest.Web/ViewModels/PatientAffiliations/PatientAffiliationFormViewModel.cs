
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using MediGest.Web.Models.Enums;

namespace MediGest.Web.ViewModels.PatientAffiliations;

public class PatientAffiliationFormViewModel
{
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    [Display(Name = "Paciente")]
    public string PatientName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná una cobertura médica.")]
    [Display(Name = "Obra social o prepaga")]
    public int HealthInsuranceId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un plan.")]
    [Display(Name = "Plan de cobertura")]
    public int HealthPlanId { get; set; }

    [Required(ErrorMessage = "El número de afiliado es obligatorio.")]
    [StringLength(50, ErrorMessage = "El número de afiliado no puede superar los 50 caracteres.")]
    [Display(Name = "Número de afiliado")]
    public string MembershipNumber { get; set; } = string.Empty;

    [Range(1, 4, ErrorMessage = "Seleccioná un tipo de beneficiario válido.")]
    [Display(Name = "Tipo de beneficiario")]
    public PatientMemberType MemberType { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Vigente desde")]
    public DateOnly ValidFrom { get; set; } =
        DateOnly.FromDateTime(DateTime.Today);

    [DataType(DataType.Date)]
    [Display(Name = "Vigente hasta")]
    public DateOnly? ValidTo { get; set; }

    [Display(Name = "Cobertura principal")]
    public bool IsPrimary { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    [Display(Name = "Observaciones")]
    public string? Notes { get; set; }

    public IEnumerable<SelectListItem> HealthInsurances { get; set; }
        = new List<SelectListItem>();

    public IEnumerable<SelectListItem> HealthPlans { get; set; }
        = new List<SelectListItem>();
}
