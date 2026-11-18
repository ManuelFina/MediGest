
using MediGest.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediGest.Web.Areas.Reception.Controllers;

[Area("Reception")]
[Authorize(Roles = "Receptionist")]
public class AgendaController : Controller
{
    private readonly IAgendaService _agendaService;

    public AgendaController(IAgendaService agendaService)
    {
        _agendaService = agendaService;
    }

    // GET: Reception/Agenda
    [HttpGet]
    public async Task<IActionResult> Index(
        DateOnly? date,
        int? professionalId)
    {
        var selectedDate = date ??
            DateOnly.FromDateTime(DateTime.Today);

        var viewModel = await _agendaService
            .GetReceptionistAgendaAsync(
                selectedDate,
                professionalId);

        return View(viewModel);
    }
}
