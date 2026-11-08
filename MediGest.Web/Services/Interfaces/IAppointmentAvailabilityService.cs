
namespace MediGest.Web.Services.Interfaces;

public interface IAppointmentAvailabilityService
{
    Task<IEnumerable<TimeOnly>> GetAvailableSlotsAsync(
        int professionalId,
        DateOnly date);
}
