
using System.ComponentModel.DataAnnotations;

namespace MediGest.Web.Models.Entities;

public class ScheduleDay
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El día de la semana es obligatorio.")]
    public DayOfWeek DayOfWeek { get; set; }

    [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "La hora de finalización es obligatoria.")]
    public TimeOnly EndTime { get; set; }

    // Professional schedule relationship
    public int ProfessionalScheduleId { get; set; }

    public ProfessionalSchedule ProfessionalSchedule { get; set; } = null!;

    public ICollection<ScheduleDay> ScheduleDays { get; set; }
    = new List<ScheduleDay>();
}
