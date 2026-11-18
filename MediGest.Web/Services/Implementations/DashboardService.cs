
using MediGest.Web.Data;
using MediGest.Web.Models.Enums;
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

    public async Task<ReceptionistDashboardViewModel> GetReceptionistDashboardAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var currentTime = TimeOnly.FromDateTime(DateTime.Now);

        var appointments = _context.Appointments
            .AsNoTracking()
            .Where(appointment => appointment.AppointmentDate == today);

        var totalPatients = await _context.Patients
            .CountAsync(patient => patient.IsActive);

        var todayAppointments = await appointments.CountAsync();

        var pendingAppointments = await appointments
            .CountAsync(appointment =>
                appointment.Status == AppointmentStatus.Pending);

        var confirmedAppointments = await appointments
            .CountAsync(appointment =>
                appointment.Status == AppointmentStatus.Confirmed);

        var waitingAppointments = await appointments
            .CountAsync(appointment =>
                appointment.Status == AppointmentStatus.Waiting);

        var upcomingAppointments = await appointments
            .Where(appointment =>
                appointment.StartTime >= currentTime &&
                appointment.Status != AppointmentStatus.Cancelled &&
                appointment.Status != AppointmentStatus.Completed &&
                appointment.Status != AppointmentStatus.NoShow)
            .OrderBy(appointment => appointment.StartTime)
            .Take(8)
            .Select(appointment => new UpcomingAppointmentViewModel
            {
                AppointmentId = appointment.Id,

                PatientName = $"{appointment.Patient.FirstName} {appointment.Patient.LastName}",

                PatientDocument = appointment.Patient.DocumentNumber,

                ProfessionalName =
                    $"{appointment.Professional.ApplicationUser.FirstName} " +
                    $"{appointment.Professional.ApplicationUser.LastName}",

                SpecialtyName = appointment.Professional.Specialty.Name,

                StartTime = appointment.StartTime,

                EndTime = appointment.EndTime,

                Status = appointment.Status == AppointmentStatus.Pending
                    ? "Pendiente"
                    : appointment.Status == AppointmentStatus.Confirmed
                        ? "Confirmado"
                        : "En espera"
            })
            .ToListAsync();

        return new ReceptionistDashboardViewModel
        {
            TotalPatients = totalPatients,
            TodayAppointments = todayAppointments,
            PendingAppointments = pendingAppointments,
            ConfirmedAppointments = confirmedAppointments,
            WaitingAppointments = waitingAppointments,
            UpcomingAppointments = upcomingAppointments
        };
    }
}
