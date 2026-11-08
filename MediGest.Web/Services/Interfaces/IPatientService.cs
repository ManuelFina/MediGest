
using MediGest.Web.Models.Entities;

namespace MediGest.Web.Services.Interfaces;

public interface IPatientService
{
    Task<IEnumerable<Patient>> GetAllAsync();

    Task<IEnumerable<Patient>> SearchAsync(string searchTerm);

    Task<Patient?> GetByIdAsync(int id);

    Task<Patient?> GetByDocumentNumberAsync(string documentNumber);

    Task<Patient> CreateAsync(Patient patient);

    Task UpdateAsync(Patient patient);

    Task DeactivateAsync(int id);

    Task ActivateAsync(int id);
}
