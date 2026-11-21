
using MediGest.Web.Models.Entities;

namespace MediGest.Web.Services.Interfaces;

public interface IPatientAffiliationService
{
    Task<IEnumerable<PatientAffiliation>> GetAllAsync();

    Task<IEnumerable<PatientAffiliation>> GetByPatientIdAsync(int patientId);

    Task<IEnumerable<PatientAffiliation>> GetCurrentByPatientIdAsync(int patientId);

    Task<PatientAffiliation?> GetByIdAsync(int id);

    Task<PatientAffiliation> CreateAsync(PatientAffiliation affiliation);

    Task UpdateAsync(int id, PatientAffiliation affiliation);

    Task SuspendAsync(int id);

    Task ReactivateAsync(int id);

    Task CancelAsync(int id);
}
