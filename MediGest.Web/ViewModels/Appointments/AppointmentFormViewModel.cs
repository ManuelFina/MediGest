
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.ViewModels.Appointments;

public class AppointmentFormViewModel
{
    [Required(ErrorMessage = "Debe seleccionar un paciente.")]
    [Display(Name = "Paciente")]
    public int PatientId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un profesional.")]
    [Display(Name = "Profesional")]
    public int ProfessionalId { get; set; }

    [Required(ErrorMessage = "La fecha del turno es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha del turno")]
    public DateOnly AppointmentDate { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un horario.")]
    [DataType(DataType.Time)]
    [Display(Name = "Horario disponible")]
    public TimeOnly StartTime { get; set; }

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
