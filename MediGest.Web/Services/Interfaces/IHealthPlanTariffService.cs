
using MediGest.Web.Models.Entities;

namespace MediGest.Web.Services.Interfaces;

public interface IHealthPlanTariffService
{
    Task<IEnumerable<HealthPlanTariff>> GetAllAsync();

    Task<IEnumerable<HealthPlanTariff>> GetByHealthPlanIdAsync(int healthPlanId);

    Task<IEnumerable<HealthPlanTariff>> GetByMedicalServiceIdAsync(int medicalServiceId);

    Task<HealthPlanTariff?> GetByIdAsync(int id);

    Task<HealthPlanTariff?> GetCurrentAsync(
        int healthPlanId,
        int medicalServiceId,
        DateOnly date);

    Task<HealthPlanTariff> CreateAsync(HealthPlanTariff tariff);

    Task UpdateAsync(int id, HealthPlanTariff tariff);

    Task CloseAsync(int id, DateOnly validTo);
}
