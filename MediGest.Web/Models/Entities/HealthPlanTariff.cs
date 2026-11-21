
using System.ComponentModel.DataAnnotations;

namespace MediGest.Web.Models.Entities;

public class HealthPlanTariff
{
    public int Id { get; set; }

    [Required]
    public int HealthPlanId { get; set; }

    public HealthPlan HealthPlan { get; set; } = null!;

    [Required]
    public int MedicalServiceId { get; set; }

    public MedicalService MedicalService { get; set; } = null!;

    [Required(ErrorMessage = "El importe de la tarifa es obligatorio.")]
    [Range(typeof(decimal), "0.01", "999999999999.99",
        ErrorMessage = "El importe debe ser mayor a cero.")]
    [Display(Name = "Importe reconocido")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "La fecha de inicio de vigencia es obligatoria.")]
    [Display(Name = "Vigente desde")]
    public DateOnly ValidFrom { get; set; }

    [Display(Name = "Vigente hasta")]
    public DateOnly? ValidTo { get; set; }

    [Display(Name = "Requiere autorización")]
    public bool RequiresAuthorization { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    [Display(Name = "Observaciones")]
    public string? Notes { get; set; }
}
