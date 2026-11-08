
using MediGest.Web.Models.Entities;

namespace MediGest.Web.Services.Interfaces;

public interface IScheduleDayService
{
    Task<IEnumerable<ScheduleDay>> GetByScheduleAsync(int scheduleId);

    Task<ScheduleDay> CreateAsync(ScheduleDay scheduleDay);

    Task UpdateAsync(ScheduleDay scheduleDay);

    Task DeleteAsync(int id);
}
