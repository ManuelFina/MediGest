
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Enums;

namespace MediGest.Web.ViewModels.PatientAffiliations;

public class PatientAffiliationListViewModel
{
    public int PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;

    public string DocumentNumber { get; set; } = string.Empty;

    public IEnumerable<PatientAffiliation> Affiliations { get; set; }
        = new List<PatientAffiliation>();

    public int TotalAffiliations => Affiliations.Count();

    public int ActiveAffiliations => Affiliations.Count(a =>
        a.Status == PatientAffiliationStatus.Active);
}
