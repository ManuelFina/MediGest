
using MediGest.Web.Models.Entities;

namespace MediGest.Web.Services.Interfaces;

public interface IHealthInsuranceService
{
    Task<IEnumerable<HealthInsurance>> GetAllAsync();

    Task<IEnumerable<HealthInsurance>> GetActiveAsync();

    Task<HealthInsurance?> GetByIdAsync(int id);

    Task<HealthInsurance?> GetByNameAsync(string name);

    Task<HealthInsurance> CreateAsync(HealthInsurance healthInsurance);

    Task UpdateAsync(int id, HealthInsurance healthInsurance);

    Task DeactivateAsync(int id);

    Task ActivateAsync(int id);
}
