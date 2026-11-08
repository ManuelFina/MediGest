
using System.ComponentModel.DataAnnotations;
using MediGest.Web.Models.Identity;

namespace MediGest.Web.Models.Entities;

public class Professional
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public string ApplicationUserId { get; set; } = string.Empty;

    public ApplicationUser ApplicationUser { get; set; } = null!;

    [Required(ErrorMessage = "La matrícula profesional es obligatoria.")]
    [StringLength(30, ErrorMessage = "La matrícula no puede superar los 30 caracteres.")]
    public string LicenseNumber { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<ProfessionalSchedule> Schedules { get; set; }
        = new List<ProfessionalSchedule>();

    public ICollection<ScheduleException> ScheduleExceptions { get; set; }
        = new List<ScheduleException>();
}
