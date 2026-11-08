
using MediGest.Web.Models.Entities;

namespace MediGest.Web.ViewModels.Professionals;

public class ProfessionalListViewModel
{
    public IEnumerable<Professional> Professionals { get; set; }
        = new List<Professional>();

    public string? SearchTerm { get; set; }

    public int TotalProfessionals { get; set; }

    public int ActiveProfessionals { get; set; }

    public int InactiveProfessionals { get; set; }
}
