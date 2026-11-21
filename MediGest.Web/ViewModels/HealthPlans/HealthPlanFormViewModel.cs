
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.ViewModels.HealthPlans;

public class HealthPlanFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La cobertura médica es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná una cobertura médica válida.")]
    [Display(Name = "Obra social / Prepaga")]
    public int HealthInsuranceId { get; set; }

    [Required(ErrorMessage = "El nombre del plan es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [Display(Name = "Nombre del plan")]
    public string Name { get; set; } = string.Empty;

    [StringLength(30, ErrorMessage = "El código no puede superar los 30 caracteres.")]
    [Display(Name = "Código")]
    public string? Code { get; set; }

    [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Display(Name = "Requiere autorización")]
    public bool RequiresAuthorization { get; set; }

    public IEnumerable<SelectListItem> HealthInsurances { get; set; }
        = new List<SelectListItem>();
}
