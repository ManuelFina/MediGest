
using MediGest.Web.Models.Entities;

namespace MediGest.Web.ViewModels.HealthPlans;

public class HealthPlanListViewModel
{
    public IEnumerable<HealthPlan> HealthPlans { get; set; }
        = new List<HealthPlan>();

    public int TotalHealthPlans { get; set; }

    public int ActiveHealthPlans { get; set; }

    public int InactiveHealthPlans { get; set; }
}
