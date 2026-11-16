
using MediGest.Web.Data;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Identity;
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardService(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<AdminDashboardViewModel> GetAdminDashboardAsync()
    {
        var users = _userManager.Users;

        var totalUsers = await users.CountAsync();

        var activeUsers = await users.CountAsync(user => user.IsActive);

        var inactiveUsers = totalUsers - activeUsers;

        var totalProfessionals = await _context.Professionals
            .CountAsync();

        var activeProfessionals = await _context.Professionals
            .CountAsync(professional => professional.IsActive);

        var totalSpecialties = await _context.Specialties
            .CountAsync();

        var activeSpecialties = await _context.Specialties
            .CountAsync(specialty => specialty.IsActive);

        var recentUsers = await users
            .AsNoTracking()
            .OrderByDescending(user => user.CreatedAt)
            .Take(5)
            .ToListAsync();

        var recentUserViewModels = new List<RecentUserViewModel>();

        foreach (var user in recentUsers)
        {
            var roles = await _userManager.GetRolesAsync(user);

            recentUserViewModels.Add(new RecentUserViewModel
            {
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? "Sin rol",
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            });
        }

        return new AdminDashboardViewModel
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            InactiveUsers = inactiveUsers,
            TotalProfessionals = totalProfessionals,
            ActiveProfessionals = activeProfessionals,
            TotalSpecialties = totalSpecialties,
            ActiveSpecialties = activeSpecialties,
            RecentUsers = recentUserViewModels
        };
    }
}
