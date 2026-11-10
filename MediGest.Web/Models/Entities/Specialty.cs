
using System.ComponentModel.DataAnnotations;

namespace MediGest.Web.Models.Entities;

public class Specialty
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la especialidad es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [Display(Name = "Especialidad")]
    public string Name { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    [Display(Name = "Activa")]
    public bool IsActive { get; set; } = true;

    public ICollection<Professional> Professionals { get; set; } = new List<Professional>();
}
