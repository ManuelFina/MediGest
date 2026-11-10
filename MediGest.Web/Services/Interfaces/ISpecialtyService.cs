
using MediGest.Web.Models.Entities;

namespace MediGest.Web.Services.Interfaces;

public interface ISpecialtyService
{
    Task<IEnumerable<Specialty>> GetAllAsync();

    Task<IEnumerable<Specialty>> GetActiveAsync();

    Task<Specialty?> GetByIdAsync(int id);

    Task<Specialty?> GetByNameAsync(string name);

    Task<Specialty> CreateAsync(Specialty specialty);

    Task UpdateAsync(int id, Specialty specialty);

    Task DeactivateAsync(int id);

    Task ActivateAsync(int id);
}
