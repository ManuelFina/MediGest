
using System.ComponentModel.DataAnnotations;

namespace MediGest.Web.ViewModels.Schedules;

public class ScheduleDayFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El día de la semana es obligatorio.")]
    [Display(Name = "Día de la semana")]
    public DayOfWeek DayOfWeek { get; set; }

    [Display(Name = "Día habilitado")]
    public bool IsEnabled { get; set; }

    [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
    [DataType(DataType.Time)]
    [Display(Name = "Hora de inicio")]
    public TimeOnly StartTime { get; set; } = new(8, 0);

    [Required(ErrorMessage = "La hora de finalización es obligatoria.")]
    [DataType(DataType.Time)]
    [Display(Name = "Hora de finalización")]
    public TimeOnly EndTime { get; set; } = new(12, 0);

    public int ProfessionalScheduleId { get; set; }
}
