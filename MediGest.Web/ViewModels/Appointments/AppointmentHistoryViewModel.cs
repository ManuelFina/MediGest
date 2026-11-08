
using System.ComponentModel.DataAnnotations;

namespace MediGest.Web.ViewModels.Appointments;

public class AppointmentHistoryViewModel
{
    public int AppointmentId { get; set; }

    [Display(Name = "Paciente")]
    public string PatientName { get; set; } = string.Empty;

    [Display(Name = "Profesional")]
    public string ProfessionalName { get; set; } = string.Empty;

    [Display(Name = "Fecha del turno")]
    public DateOnly AppointmentDate { get; set; }

    public IEnumerable<AppointmentHistoryItemViewModel> History { get; set; }
        = new List<AppointmentHistoryItemViewModel>();
}

public class AppointmentHistoryItemViewModel
{
    public string Description { get; set; } = string.Empty;

    [Display(Name = "Fecha del cambio")]
    public DateTime ChangedAt { get; set; }

    [Display(Name = "Realizado por")]
    public string ChangedByUserName { get; set; } = string.Empty;
}
