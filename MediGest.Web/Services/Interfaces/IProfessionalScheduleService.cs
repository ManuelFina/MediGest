
using MediGest.Web.Models.Entities;

namespace MediGest.Web.Services.Interfaces;

public interface IProfessionalScheduleService
{
    Task<IEnumerable<ProfessionalSchedule>> GetByProfessionalAsync(
        int professionalId);

    Task<ProfessionalSchedule?> GetByIdAsync(int id);

    Task<ProfessionalSchedule> CreateAsync(
        ProfessionalSchedule schedule);

    Task UpdateAsync(ProfessionalSchedule schedule);

    Task DeleteAsync(int id);
}
