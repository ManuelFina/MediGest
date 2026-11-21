
using System.ComponentModel.DataAnnotations;
using MediGest.Web.Models.Enums;

namespace MediGest.Web.Models.Entities;

public class PatientAffiliation
{
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    public Patient Patient { get; set; } = null!;

    [Required]
    public int HealthInsuranceId { get; set; }

    public HealthInsurance HealthInsurance { get; set; } = null!;

    [Required]
    public int HealthPlanId { get; set; }

    public HealthPlan HealthPlan { get; set; } = null!;

    [Required(ErrorMessage = "El número de afiliado es obligatorio.")]
    [StringLength(50, ErrorMessage = "El número de afiliado no puede superar los 50 caracteres.")]
    public string MembershipNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de beneficiario es obligatorio.")]
    public PatientMemberType MemberType { get; set; }

    [Required(ErrorMessage = "La fecha de inicio de vigencia es obligatoria.")]
    public DateOnly ValidFrom { get; set; }

    public DateOnly? ValidTo { get; set; }

    public PatientAffiliationStatus Status { get; set; }
        = PatientAffiliationStatus.Active;

    public bool IsPrimary { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    public string? Notes { get; set; }
}
