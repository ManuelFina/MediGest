
using System.ComponentModel.DataAnnotations;
using MediGest.Web.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.ViewModels.Appointments;

public class AppointmentFormViewModel
{
    [Required(ErrorMessage = "El paciente es obligatorio.")]
    [Display(Name = "Paciente")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "El profesional es obligatorio.")]
    [Display(Name = "Profesional")]
    public int ProfessionalId { get; set; }

    [Required(ErrorMessage = "La fecha del turno es obligatoria.")]
    [Display(Name = "Fecha del turno")]
    public DateOnly AppointmentDate { get; set; }

    [Required(ErrorMessage = "El horario es obligatorio.")]
    [Display(Name = "Horario disponible")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "El estado del turno es obligatorio.")]
    [EnumDataType(typeof(AppointmentStatus),
        ErrorMessage = "Seleccione un estado válido.")]
    [Display(Name = "Estado inicial")]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    [Display(Name = "Observaciones")]
    public string? Notes { get; set; }

    public IEnumerable<SelectListItem> AvailablePatients { get; set; }
        = new List<SelectListItem>();

    public IEnumerable<SelectListItem> AvailableProfessionals { get; set; }
        = new List<SelectListItem>();

    public IEnumerable<SelectListItem> AvailableSlots { get; set; }
        = new List<SelectListItem>();
}
