
using MediGest.Web.Models.Entities;

namespace MediGest.Web.ViewModels.HealthInsurances;

public class HealthInsuranceListViewModel
{
    public IEnumerable<HealthInsurance> HealthInsurances { get; set; }
        = new List<HealthInsurance>();

    public int TotalHealthInsurances { get; set; }

    public int ActiveHealthInsurances { get; set; }

    public int InactiveHealthInsurances { get; set; }
}
