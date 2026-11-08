
using MediGest.Web.Models.Entities;

namespace MediGest.Web.Services.Interfaces;

public interface IScheduleExceptionService
{
    Task<IEnumerable<ScheduleException>> GetByProfessionalAsync(
        int professionalId);

    Task<ScheduleException?> GetByIdAsync(int id);

    Task<ScheduleException> CreateAsync(
        ScheduleException scheduleException);

    Task UpdateAsync(ScheduleException scheduleException);

    Task DeleteAsync(int id);
}
