
using MediGest.Web.Models.Entities;

namespace MediGest.Web.Services.Interfaces;

public interface IMedicalService
{
    Task<IEnumerable<MedicalService>> GetAllAsync();

    Task<IEnumerable<MedicalService>> GetActiveAsync();

    Task<MedicalService?> GetByIdAsync(int id);

    Task<MedicalService?> GetByNameAsync(string name);

    Task<MedicalService> CreateAsync(MedicalService medicalService);

    Task UpdateAsync(int id, MedicalService medicalService);

    Task DeactivateAsync(int id);

    Task ActivateAsync(int id);
}
