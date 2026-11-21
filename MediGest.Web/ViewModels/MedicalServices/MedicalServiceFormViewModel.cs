
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.ViewModels.MedicalServices;

public class MedicalServiceFormViewModel
{
    public int Id { get; set; }

    [StringLength(30, ErrorMessage = "El código no puede superar los 30 caracteres.")]
    [Display(Name = "Código")]
    public string? Code { get; set; }

    [Required(ErrorMessage = "El nombre de la prestación es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    [Display(Name = "Nombre de la prestación")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Display(Name = "Especialidad")]
    public int? SpecialtyId { get; set; }

    [Range(0.01, 999999999999.99,
        ErrorMessage = "El valor particular debe ser mayor a cero.")]
    public decimal PrivatePrice { get; set; }

    public IEnumerable<SelectListItem> Specialties { get; set; }
        = new List<SelectListItem>();
}
