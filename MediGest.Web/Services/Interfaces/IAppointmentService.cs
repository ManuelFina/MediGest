
using MediGest.Web.Models.Entities;

namespace MediGest.Web.Services.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<Appointment>> GetAllAsync();

    Task<IEnumerable<Appointment>> GetByDateAsync(DateOnly date);

    Task<IEnumerable<Appointment>> GetByProfessionalAsync(
        int professionalId,
        DateOnly? date = null);

    Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId);

    Task<Appointment?> GetByIdAsync(int id);

    Task<Appointment> CreateAsync(
        Appointment appointment,
        string changedByUserId);

    Task ConfirmAsync(int id, string changedByUserId);

    Task MarkAsWaitingAsync(int id, string changedByUserId);

    Task CancelAsync(
        int id,
        string reason,
        string changedByUserId);

    Task RescheduleAsync(
        int id,
        DateOnly newDate,
        TimeOnly newStartTime,
        string changedByUserId);

    Task CompleteAsync(int id, string changedByUserId);

    Task MarkAsNoShowAsync(int id, string changedByUserId);
}
