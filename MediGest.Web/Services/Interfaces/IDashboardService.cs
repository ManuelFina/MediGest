
using MediGest.Web.ViewModels.Dashboard;

namespace MediGest.Web.Services.Interfaces;

public interface IDashboardService
{
    Task<AdminDashboardViewModel> GetAdminDashboardAsync();

    Task<ReceptionistDashboardViewModel> GetReceptionistDashboardAsync();
}
