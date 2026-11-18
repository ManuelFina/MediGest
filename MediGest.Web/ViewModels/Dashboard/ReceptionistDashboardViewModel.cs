
using MediGest.Web.ViewModels.Appointments;

namespace MediGest.Web.ViewModels.Dashboard;

public class ReceptionistDashboardViewModel
{
    public int TotalPatients { get; set; }

    public int TodayAppointments { get; set; }

    public int PendingAppointments { get; set; }

    public int ConfirmedAppointments { get; set; }

    public int WaitingAppointments { get; set; }

    public IEnumerable<UpcomingAppointmentViewModel> UpcomingAppointments { get; set; }
        = new List<UpcomingAppointmentViewModel>();

    public DailyAgendaViewModel? DailyAgenda { get; set; }
}

public class UpcomingAppointmentViewModel
{
    public int AppointmentId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string PatientDocument { get; set; } = string.Empty;

    public string ProfessionalName { get; set; } = string.Empty;

    public string SpecialtyName { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Status { get; set; } = string.Empty;
}
