
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.ViewModels.Schedules;

public class ScheduleDayFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un día.")]
    [Display(Name = "Día de la semana")]
    public DayOfWeek DayOfWeek { get; set; }

    [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
    [DataType(DataType.Time)]
    [Display(Name = "Hora de inicio")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "La hora de finalización es obligatoria.")]
    [DataType(DataType.Time)]
    [Display(Name = "Hora de finalización")]
    public TimeOnly EndTime { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una agenda.")]
    [Display(Name = "Agenda")]
    public int ProfessionalScheduleId { get; set; }

    public IEnumerable<SelectListItem> AvailableSchedules { get; set; }
        = new List<SelectListItem>();
}
