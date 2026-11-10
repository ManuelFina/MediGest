
using System.ComponentModel.DataAnnotations;

namespace MediGest.Web.ViewModels.Specialties;

public class SpecialtyFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la especialidad es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [Display(Name = "Nombre de la especialidad")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }
}
