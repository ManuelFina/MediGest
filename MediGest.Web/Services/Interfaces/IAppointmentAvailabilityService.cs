
namespace MediGest.Web.Services.Interfaces;

public interface IAppointmentAvailabilityService
{
    Task<IEnumerable<TimeOnly>> GetAvailableSlotsAsync(
        int professionalId,
        DateOnly date,
        int? excludedAppointmentId = null);

    Task<IEnumerable<AvailableTimeSlot>> GetAvailableTimeSlotsAsync(
        int professionalId,
        DateOnly date,
        int? excludedAppointmentId = null);
}

public class AvailableTimeSlot
{
    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }
}
