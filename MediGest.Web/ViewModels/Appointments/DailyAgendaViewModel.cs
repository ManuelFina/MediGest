
namespace MediGest.Web.ViewModels.Appointments;

public class DailyAgendaViewModel
{
    public DateOnly SelectedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    // Null means all professionals.
    public int? SelectedProfessionalId { get; set; }

    public bool CanSelectProfessional { get; set; }

    public IEnumerable<AgendaProfessionalViewModel> Professionals { get; set; }
        = new List<AgendaProfessionalViewModel>();

    public IEnumerable<DailyAppointmentViewModel> Appointments { get; set; }
        = new List<DailyAppointmentViewModel>();

    public IEnumerable<AvailableAgendaSlotViewModel> AvailableSlots { get; set; }
        = new List<AvailableAgendaSlotViewModel>();
}

public class AgendaProfessionalViewModel
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string SpecialtyName { get; set; } = string.Empty;
}

public class DailyAppointmentViewModel
{
    public int Id { get; set; }

    public int ProfessionalId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string PatientDocument { get; set; } = string.Empty;

    public string ProfessionalName { get; set; } = string.Empty;

    public string SpecialtyName { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class AvailableAgendaSlotViewModel
{
    public int ProfessionalId { get; set; }

    public string ProfessionalName { get; set; } = string.Empty;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }
}
