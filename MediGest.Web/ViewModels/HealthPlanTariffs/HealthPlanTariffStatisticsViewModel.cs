namespace MediGest.Web.ViewModels.HealthPlanTariffs;

public class HealthPlanTariffStatisticsViewModel
{
    public int TotalTariffs { get; set; }

    public int CurrentTariffs { get; set; }

    public int ExpiredTariffs { get; set; }

    public int FutureTariffs { get; set; }
}