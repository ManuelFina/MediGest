
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.ViewModels.Schedules;

public class ProfessionalScheduleFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la agenda es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [Display(Name = "Nombre de la agenda")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de inicio")]
    public DateOnly StartDate { get; set; }

    [Required(ErrorMessage = "La fecha de finalización es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de finalización")]
    public DateOnly EndDate { get; set; }

    [Required(ErrorMessage = "La duración del turno es obligatoria.")]
    [Range(5, 240, ErrorMessage = "La duración debe estar entre 5 y 240 minutos.")]
    [Display(Name = "Duración del turno (minutos)")]
    public int AppointmentDurationMinutes { get; set; } = 30;

    [Required(ErrorMessage = "Debe seleccionar un profesional.")]
    [Display(Name = "Profesional")]
    public int ProfessionalId { get; set; }

    [Display(Name = "Agenda activa")]
    public bool IsActive { get; set; } = true;

    public IEnumerable<SelectListItem> AvailableProfessionals { get; set; }
        = new List<SelectListItem>();
}
