
using MediGest.Web.Models.Entities;

namespace MediGest.Web.ViewModels.Patients;

public class PatientListViewModel
{
    public IEnumerable<Patient> Patients { get; set; }
        = new List<Patient>();

    public string? SearchTerm { get; set; }

    public int TotalPatients { get; set; }

    public int ActivePatients { get; set; }

    public int InactivePatients { get; set; }
}
