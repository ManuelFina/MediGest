
using MediGest.Web.Models;
using MediGest.Web.Models.Identity;
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MediGest.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IDashboardService _dashboardService;
    private readonly IAgendaService _agendaService;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        IDashboardService dashboardService,
        IAgendaService agendaService,
        UserManager<ApplicationUser> userManager)
    {
        _dashboardService = dashboardService;
        _agendaService = agendaService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        ViewBag.UserName = user != null
            ? $"{user.FirstName} {user.LastName}".Trim()
            : "Usuario";

        var dashboard = new DashboardViewModel();

        if (User.IsInRole(ApplicationRoles.Administrator))
        {
            dashboard.AdminDashboard =
                await _dashboardService.GetAdminDashboardAsync();
        }
        else if (User.IsInRole(ApplicationRoles.Receptionist))
        {
            var receptionistDashboard =
                await _dashboardService.GetReceptionistDashboardAsync();

            receptionistDashboard.DailyAgenda =
                await _agendaService.GetReceptionistAgendaAsync(
                    DateOnly.FromDateTime(DateTime.Today));

            dashboard.ReceptionistDashboard = receptionistDashboard;
        }

        return View(dashboard);
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}
