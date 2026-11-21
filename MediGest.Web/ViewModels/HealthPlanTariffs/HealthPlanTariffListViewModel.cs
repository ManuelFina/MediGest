
using MediGest.Web.Models.Entities;

namespace MediGest.Web.ViewModels.HealthPlanTariffs;

public class HealthPlanTariffListViewModel
{
    public IEnumerable<HealthPlanTariff> Tariffs { get; set; }
        = new List<HealthPlanTariff>();

    public int TotalTariffs { get; set; }

    public int CurrentTariffs { get; set; }

    public int ExpiredTariffs { get; set; }

    public int FutureTariffs { get; set; }
}
