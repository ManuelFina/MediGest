
namespace MediGest.Web.ViewModels.Specialties;

public class SpecialtyListViewModel
{
    public IEnumerable<SpecialtyItemViewModel> Specialties { get; set; }
        = new List<SpecialtyItemViewModel>();

    public string? SearchTerm { get; set; }

    public int TotalSpecialties { get; set; }

    public int ActiveSpecialties { get; set; }

    public int InactiveSpecialties { get; set; }
}

public class SpecialtyItemViewModel
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int ProfessionalsCount { get; set; }
}
