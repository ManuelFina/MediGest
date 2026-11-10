
using MediGest.Web.Models.Entities;

namespace MediGest.Web.Services.Interfaces;

public interface IProfessionalService
{
    Task<IEnumerable<Professional>> GetAllAsync();

    Task<IEnumerable<Professional>> SearchAsync(string searchTerm);

    Task<Professional?> GetByIdAsync(int id);

    Task<Professional?> GetByApplicationUserIdAsync(string applicationUserId);

    Task<Professional> CreateAsync(Professional professional);

    Task UpdateAsync(Professional professional);

    Task DeactivateAsync(int id);

    Task ActivateAsync(int id);
}
