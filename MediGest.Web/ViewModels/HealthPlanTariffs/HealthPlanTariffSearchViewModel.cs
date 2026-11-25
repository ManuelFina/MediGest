using MediGest.Web.Models.Entities;

namespace MediGest.Web.ViewModels.HealthPlanTariffs;

public class HealthPlanTariffSearchViewModel
{
    public IEnumerable<HealthPlanTariff> Tariffs { get; set; }
        = Enumerable.Empty<HealthPlanTariff>();

    public string? SearchTerm { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 25;

    public int TotalCount { get; set; }

    public int TotalPages =>
        (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;
}