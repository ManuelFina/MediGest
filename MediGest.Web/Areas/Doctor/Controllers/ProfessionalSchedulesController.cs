
using System.Security.Claims;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.Schedules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediGest.Web.Areas.Doctor.Controllers;

[Area("Doctor")]
[Authorize(Roles = "Doctor")]
public class ProfessionalSchedulesController : Controller
{
    private readonly IProfessionalScheduleService _scheduleService;
    private readonly IProfessionalService _professionalService;

    public ProfessionalSchedulesController(
        IProfessionalScheduleService scheduleService,
        IProfessionalService professionalService)
    {
        _scheduleService = scheduleService;
        _professionalService = professionalService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var professional = await GetCurrentProfessionalAsync();

        if (professional is null)
        {
            return Forbid();
        }

        var schedules = await _scheduleService
            .GetByProfessionalAsync(professional.Id);

        return View(schedules);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ProfessionalScheduleFormViewModel
        {
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddMonths(6)),
            AppointmentDurationMinutes = 30,
            ScheduleDays = CreateDefaultScheduleDays()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ProfessionalScheduleFormViewModel viewModel)
    {
        var professional = await GetCurrentProfessionalAsync();

        if (professional is null)
        {
            return Forbid();
        }

        var enabledDays = viewModel.ScheduleDays
            .Where(day => day.IsEnabled)
            .ToList();

        ValidateScheduleDays(enabledDays);

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var schedule = new ProfessionalSchedule
        {
            Name = viewModel.Name,
            StartDate = viewModel.StartDate,
            EndDate = viewModel.EndDate,
            AppointmentDurationMinutes =
                viewModel.AppointmentDurationMinutes,
            ProfessionalId = professional.Id,
            IsActive = viewModel.IsActive,
            ScheduleDays = MapScheduleDays(enabledDays)
        };

        try
        {
            await _scheduleService.CreateAsync(schedule);

            TempData["Success"] =
                "Agenda creada correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var professional = await GetCurrentProfessionalAsync();

        if (professional is null)
        {
            return Forbid();
        }

        var schedule = await _scheduleService.GetByIdAsync(id);

        if (schedule is null)
        {
            return NotFound();
        }

        if (schedule.ProfessionalId != professional.Id)
        {
            return Forbid();
        }

        var configuredDays = schedule.ScheduleDays
            .ToDictionary(day => day.DayOfWeek);

        var viewModel = new ProfessionalScheduleFormViewModel
        {
            Id = schedule.Id,
            Name = schedule.Name,
            StartDate = schedule.StartDate,
            EndDate = schedule.EndDate,
            AppointmentDurationMinutes =
                schedule.AppointmentDurationMinutes,
            ProfessionalId = professional.Id,
            IsActive = schedule.IsActive,

            ScheduleDays = CreateDefaultScheduleDays()
                .Select(day =>
                {
                    if (configuredDays.TryGetValue(
                        day.DayOfWeek,
                        out var configuredDay))
                    {
                        day.IsEnabled = true;
                        day.StartTime = configuredDay.StartTime;
                        day.EndTime = configuredDay.EndTime;
                    }

                    return day;
                })
                .ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        ProfessionalScheduleFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        var professional = await GetCurrentProfessionalAsync();

        if (professional is null)
        {
            return Forbid();
        }

        var existingSchedule = await _scheduleService.GetByIdAsync(id);

        if (existingSchedule is null)
        {
            return NotFound();
        }

        if (existingSchedule.ProfessionalId != professional.Id)
        {
            return Forbid();
        }

        var enabledDays = viewModel.ScheduleDays
            .Where(day => day.IsEnabled)
            .ToList();

        ValidateScheduleDays(enabledDays);

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var schedule = new ProfessionalSchedule
        {
            Id = id,
            Name = viewModel.Name,
            StartDate = viewModel.StartDate,
            EndDate = viewModel.EndDate,
            AppointmentDurationMinutes =
                viewModel.AppointmentDurationMinutes,
            ProfessionalId = professional.Id,
            IsActive = viewModel.IsActive,
            ScheduleDays = MapScheduleDays(enabledDays)
        };

        try
        {
            await _scheduleService.UpdateAsync(schedule);

            TempData["Success"] =
                "Agenda actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var professional = await GetCurrentProfessionalAsync();

        if (professional is null)
        {
            return Forbid();
        }

        var schedule = await _scheduleService.GetByIdAsync(id);

        if (schedule is null)
        {
            return NotFound();
        }

        if (schedule.ProfessionalId != professional.Id)
        {
            return Forbid();
        }

        try
        {
            await _scheduleService.DeleteAsync(id);

            TempData["Success"] =
                "Agenda eliminada correctamente.";
        }
        catch (BusinessException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<Professional?> GetCurrentProfessionalAsync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await _professionalService
            .GetByApplicationUserIdAsync(userId);
    }

    private static List<ScheduleDayFormViewModel>
        CreateDefaultScheduleDays()
    {
        return Enumerable
            .Range(0, 7)
            .Select(i => new ScheduleDayFormViewModel
            {
                DayOfWeek = (DayOfWeek)((i + 1) % 7),
                IsEnabled = false
            })
            .ToList();
    }

    private void ValidateScheduleDays(
        List<ScheduleDayFormViewModel> enabledDays)
    {
        if (enabledDays.Count == 0)
        {
            ModelState.AddModelError(
                nameof(ProfessionalScheduleFormViewModel.ScheduleDays),
                "Debés seleccionar al menos un día de atención.");
        }

        foreach (var day in enabledDays)
        {
            if (day.StartTime >= day.EndTime)
            {
                ModelState.AddModelError(
                    nameof(ProfessionalScheduleFormViewModel.ScheduleDays),
                    $"El horario de {day.DayOfWeek} no es válido. " +
                    "La hora de inicio debe ser anterior a la hora de finalización.");
            }
        }
    }

    private static List<ScheduleDay> MapScheduleDays(
        IEnumerable<ScheduleDayFormViewModel> days)
    {
        return days.Select(day => new ScheduleDay
        {
            DayOfWeek = day.DayOfWeek,
            StartTime = day.StartTime,
            EndTime = day.EndTime
        }).ToList();
    }
}
