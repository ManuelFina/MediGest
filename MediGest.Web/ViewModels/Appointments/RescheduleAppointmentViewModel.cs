
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.ViewModels.Appointments;

public class RescheduleAppointmentViewModel
{
    public int AppointmentId { get; set; }

    public int ProfessionalId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string ProfessionalName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha del turno es obligatoria.")]
    [Display(Name = "Nueva fecha")]
    public DateOnly NewDate { get; set; }

    [Required(ErrorMessage = "El horario es obligatorio.")]
    [Display(Name = "Nuevo horario")]
    public TimeOnly NewStartTime { get; set; }

    public IEnumerable<SelectListItem> AvailableSlots { get; set; }
        = new List<SelectListItem>();
}
