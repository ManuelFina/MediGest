
using MediGest.Web.Models.Entities;

namespace MediGest.Web.Services.Interfaces;

public interface IHealthPlanService
{
    Task<IEnumerable<HealthPlan>> GetAllAsync();

    Task<IEnumerable<HealthPlan>> GetActiveAsync();

    Task<HealthPlan?> GetByIdAsync(int id);

    Task<HealthPlan?> GetByNameAsync(string name);

    Task<HealthPlan> CreateAsync(HealthPlan healthPlan);

    Task UpdateAsync(int id, HealthPlan healthPlan);

    Task DeactivateAsync(int id);

    Task ActivateAsync(int id);
}
