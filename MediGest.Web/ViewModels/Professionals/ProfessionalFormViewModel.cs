
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.ViewModels.Professionals;

public class ProfessionalFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un usuario.")]
    [Display(Name = "Usuario")]
    public string ApplicationUserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "La matrícula profesional es obligatoria.")]
    [StringLength(30, ErrorMessage = "La matrícula no puede superar los 30 caracteres.")]
    [Display(Name = "Matrícula profesional")]
    public string LicenseNumber { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una especialidad.")]
    [Display(Name = "Especialidad")]
    public int SpecialtyId { get; set; }

    [Display(Name = "Estado")]
    public bool IsActive { get; set; } = true;

    public IEnumerable<SelectListItem> AvailableUsers { get; set; }
        = new List<SelectListItem>();

    public IEnumerable<SelectListItem> AvailableSpecialties { get; set; }
        = new List<SelectListItem>();
}
