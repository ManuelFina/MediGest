using MediGest.Web.Models.Entities;
using MediGest.Web.ViewModels.HealthPlanTariffs;

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

    Task<HealthPlanTariffSearchViewModel> GetPageAsync(
        int page,
        int pageSize);

    Task<HealthPlanTariffSearchViewModel> SearchAsync(
        string? searchTerm,
        int page,
        int pageSize);

    Task<HealthPlanTariffStatisticsViewModel> GetStatisticsAsync(
        DateOnly today);

    Task<HealthPlanTariff> CreateAsync(HealthPlanTariff tariff);

    Task UpdateAsync(int id, HealthPlanTariff tariff);

    Task CloseAsync(int id, DateOnly validTo);
}