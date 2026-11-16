
namespace MediGest.Web.ViewModels.Dashboard;

public class AdminDashboardViewModel
{
    // Estadísticas de usuarios
    public int TotalUsers { get; set; }

    public int ActiveUsers { get; set; }

    public int InactiveUsers { get; set; }

    // Estadísticas de profesionales
    public int TotalProfessionals { get; set; }

    public int ActiveProfessionals { get; set; }

    // Estadísticas de especialidades
    public int TotalSpecialties { get; set; }

    public int ActiveSpecialties { get; set; }

    // Últimos usuarios registrados
    public IEnumerable<RecentUserViewModel> RecentUsers { get; set; }
        = new List<RecentUserViewModel>();
}

public class RecentUserViewModel
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }
}
