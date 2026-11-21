
using MediGest.Web.Models.Entities;

namespace MediGest.Web.ViewModels.MedicalServices;

public class MedicalServiceListViewModel
{
    public IEnumerable<MedicalService> MedicalServices { get; set; }
        = new List<MedicalService>();

    public int TotalMedicalServices { get; set; }

    public int ActiveMedicalServices { get; set; }

    public int InactiveMedicalServices { get; set; }
}
