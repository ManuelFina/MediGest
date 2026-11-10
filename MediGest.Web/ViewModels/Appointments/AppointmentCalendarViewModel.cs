
using MediGest.Web.Models.Entities;

namespace MediGest.Web.ViewModels.Appointments;

public class AppointmentCalendarViewModel
{
    public DateOnly SelectedDate { get; set; }

    public DateOnly WeekStartDate { get; set; }

    public DateOnly WeekEndDate { get; set; }

    public IEnumerable<Appointment> Appointments { get; set; }
        = new List<Appointment>();

    public IEnumerable<Professional> Professionals { get; set; }
        = new List<Professional>();

    public IEnumerable<DateOnly> WeekDays { get; set; }
        = new List<DateOnly>();

    public int StartHour { get; set; } = 8;

    public int EndHour { get; set; } = 18;

    public int TotalAppointments { get; set; }

    public int PendingAppointments { get; set; }

    public int ConfirmedAppointments { get; set; }

    public int CancelledAppointments { get; set; }
}
