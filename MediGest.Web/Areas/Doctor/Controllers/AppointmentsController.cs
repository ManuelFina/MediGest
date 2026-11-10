
using System.Security.Claims;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Enums;
using MediGest.Web.Models.Identity;
using MediGest.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediGest.Web.Areas.Doctor.Controllers;

[Area("Doctor")]
[Authorize(Roles = ApplicationRoles.Doctor)]
public class AppointmentsController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IProfessionalService _professionalService;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IProfessionalService professionalService)
    {
        _appointmentService = appointmentService;
        _professionalService = professionalService;
    }

    // GET: Doctor/Appointments
    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? date)
    {
        var professional = await GetCurrentProfessionalAsync();

        if (professional is null)
        {
            return Forbid();
        }

        var selectedDate = date ??
            DateOnly.FromDateTime(DateTime.Today);

        var appointments = await _appointmentService
            .GetByProfessionalAsync(
                professional.Id,
                selectedDate);

        ViewBag.SelectedDate = selectedDate;
        ViewBag.ProfessionalName =
            $"{professional.ApplicationUser.LastName}, " +
            $"{professional.ApplicationUser.FirstName}";

        return View(appointments);
    }

    // POST: Doctor/Appointments/Complete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var professional = await GetCurrentProfessionalAsync();

        if (professional is null)
        {
            return Forbid();
        }

        var appointment = await _appointmentService.GetByIdAsync(id);

        if (appointment is null)
        {
            return NotFound();
        }

        // El médico solamente puede completar sus propios turnos.
        if (appointment.ProfessionalId != professional.Id)
        {
            return Forbid();
        }

        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Forbid();
        }

        try
        {
            await _appointmentService.CompleteAsync(id, userId);

            TempData["Success"] =
                "Atención médica completada correctamente.";
        }
        catch (BusinessException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(
            nameof(Index),
            new { date = appointment.AppointmentDate });
    }

    private async Task<Professional?> GetCurrentProfessionalAsync()
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await _professionalService
            .GetByApplicationUserIdAsync(userId);
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? string.Empty;
    }
}
