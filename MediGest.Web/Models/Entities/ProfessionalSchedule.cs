using System.ComponentModel.DataAnnotations;

namespace MediGest.Web.Models.Entities;

public class ProfessionalSchedule
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la agenda es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateOnly StartDate { get; set; }

    [Required(ErrorMessage = "La fecha de finalización es obligatoria.")]
    public DateOnly EndDate { get; set; }

    [Required(ErrorMessage = "La duración del turno es obligatoria.")]
    [Range(5, 240, ErrorMessage = "La duración debe estar entre 5 y 240 minutos.")]
    public int AppointmentDurationMinutes { get; set; }

    public bool IsActive { get; set; } = true;

    // Professional relationship
    [Required]
    public int ProfessionalId { get; set; }

    public Professional Professional { get; set; } = null!;

    // Schedule days relationship
    public ICollection<ScheduleDay> ScheduleDays { get; set; }
        = new List<ScheduleDay>();
}