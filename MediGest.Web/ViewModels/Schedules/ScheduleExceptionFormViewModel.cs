
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.ViewModels.Schedules;

public class ScheduleExceptionFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un profesional.")]
    [Display(Name = "Profesional")]
    public int ProfessionalId { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de inicio")]
    public DateOnly StartDate { get; set; }

    [Required(ErrorMessage = "La fecha de finalización es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de finalización")]
    public DateOnly EndDate { get; set; }

    [DataType(DataType.Time)]
    [Display(Name = "Hora de inicio")]
    public TimeOnly? StartTime { get; set; }

    [DataType(DataType.Time)]
    [Display(Name = "Hora de finalización")]
    public TimeOnly? EndTime { get; set; }

    [Required(ErrorMessage = "El motivo es obligatorio.")]
    [StringLength(200, ErrorMessage = "El motivo no puede superar los 200 caracteres.")]
    [Display(Name = "Motivo")]
    public string Reason { get; set; } = string.Empty;

    public IEnumerable<SelectListItem> AvailableProfessionals { get; set; }
        = new List<SelectListItem>();
}
