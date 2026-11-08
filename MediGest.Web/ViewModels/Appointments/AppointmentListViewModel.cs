
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Enums;

namespace MediGest.Web.ViewModels.Appointments;

public class AppointmentListViewModel
{
    public IEnumerable<Appointment> Appointments { get; set; }
        = new List<Appointment>();

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int? ProfessionalId { get; set; }

    public int? PatientId { get; set; }

    public AppointmentStatus? Status { get; set; }

    public string? SearchTerm { get; set; }

    public int TotalAppointments { get; set; }

    public int PendingAppointments { get; set; }

    public int ConfirmedAppointments { get; set; }

    public int CancelledAppointments { get; set; }
}
