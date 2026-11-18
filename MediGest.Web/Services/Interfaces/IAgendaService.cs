
using MediGest.Web.ViewModels.Appointments;

namespace MediGest.Web.Services.Interfaces;

public interface IAgendaService
{
    Task<DailyAgendaViewModel> GetReceptionistAgendaAsync(
        DateOnly date,
        int? professionalId = null);

    Task<DailyAgendaViewModel> GetDoctorAgendaAsync(
        DateOnly date,
        string applicationUserId);
}
