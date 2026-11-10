
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
public class ScheduleDaysController : Controller
{
    private readonly IScheduleDayService _scheduleDayService;
    private readonly IProfessionalScheduleService _scheduleService;
    private readonly IProfessionalService _professionalService;

    public ScheduleDaysController(
        IScheduleDayService scheduleDayService,
        IProfessionalScheduleService scheduleService,
        IProfessionalService professionalService)
    {
        _scheduleDayService = scheduleDayService;
        _scheduleService = scheduleService;
        _professionalService = professionalService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int scheduleId)
    {
        var schedule = await GetOwnedScheduleAsync(scheduleId);

        if (schedule is null)
        {
            return NotFound();
        }

        var scheduleDays = await _scheduleDayService
            .GetByScheduleAsync(scheduleId);

        ViewBag.Schedule = schedule;

        return View(scheduleDays);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int scheduleId)
    {
        var schedule = await GetOwnedScheduleAsync(scheduleId);

        if (schedule is null)
        {
            return NotFound();
        }

        var viewModel = new ScheduleDayFormViewModel
        {
            ProfessionalScheduleId = scheduleId
        };

        ViewBag.Schedule = schedule;

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int scheduleId,
        ScheduleDayFormViewModel viewModel)
    {
        if (scheduleId != viewModel.ProfessionalScheduleId)
        {
            return BadRequest();
        }

        var schedule = await GetOwnedScheduleAsync(scheduleId);

        if (schedule is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Schedule = schedule;
            return View(viewModel);
        }

        var scheduleDay = new ScheduleDay
        {
            DayOfWeek = viewModel.DayOfWeek,
            StartTime = viewModel.StartTime,
            EndTime = viewModel.EndTime,
            ProfessionalScheduleId = scheduleId
        };

        try
        {
            await _scheduleDayService.CreateAsync(scheduleDay);

            TempData["Success"] =
                "Bloque horario agregado correctamente.";

            return RedirectToAction(
                nameof(Index),
                new { scheduleId });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            ViewBag.Schedule = schedule;
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int scheduleId, int id)
    {
        var schedule = await GetOwnedScheduleAsync(scheduleId);

        if (schedule is null)
        {
            return NotFound();
        }

        var scheduleDays = await _scheduleDayService
            .GetByScheduleAsync(scheduleId);

        var scheduleDay = scheduleDays.FirstOrDefault(d => d.Id == id);

        if (scheduleDay is null)
        {
            return NotFound();
        }

        var viewModel = new ScheduleDayFormViewModel
        {
            Id = scheduleDay.Id,
            DayOfWeek = scheduleDay.DayOfWeek,
            StartTime = scheduleDay.StartTime,
            EndTime = scheduleDay.EndTime,
            ProfessionalScheduleId = scheduleId
        };

        ViewBag.Schedule = schedule;

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int scheduleId,
        int id,
        ScheduleDayFormViewModel viewModel)
    {
        if (id != viewModel.Id ||
            scheduleId != viewModel.ProfessionalScheduleId)
        {
            return BadRequest();
        }

        var schedule = await GetOwnedScheduleAsync(scheduleId);

        if (schedule is null)
        {
            return NotFound();
        }

        var scheduleDays = await _scheduleDayService
            .GetByScheduleAsync(scheduleId);

        if (!scheduleDays.Any(d => d.Id == id))
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Schedule = schedule;
            return View(viewModel);
        }

        var scheduleDay = new ScheduleDay
        {
            Id = id,
            DayOfWeek = viewModel.DayOfWeek,
            StartTime = viewModel.StartTime,
            EndTime = viewModel.EndTime,
            ProfessionalScheduleId = scheduleId
        };

        try
        {
            await _scheduleDayService.UpdateAsync(scheduleDay);

            TempData["Success"] =
                "Bloque horario actualizado correctamente.";

            return RedirectToAction(
                nameof(Index),
                new { scheduleId });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            ViewBag.Schedule = schedule;
            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int scheduleId, int id)
    {
        var schedule = await GetOwnedScheduleAsync(scheduleId);

        if (schedule is null)
        {
            return NotFound();
        }

        var scheduleDays = await _scheduleDayService
            .GetByScheduleAsync(scheduleId);

        if (!scheduleDays.Any(d => d.Id == id))
        {
            return NotFound();
        }

        try
        {
            await _scheduleDayService.DeleteAsync(id);

            TempData["Success"] =
                "Bloque horario eliminado correctamente.";
        }
        catch (BusinessException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(
            nameof(Index),
            new { scheduleId });
    }

    private async Task<ProfessionalSchedule?> GetOwnedScheduleAsync(
        int scheduleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var professional = await _professionalService
            .GetByApplicationUserIdAsync(userId);

        if (professional is null)
        {
            return null;
        }

        var schedule = await _scheduleService.GetByIdAsync(scheduleId);

        if (schedule is null ||
            schedule.ProfessionalId != professional.Id)
        {
            return null;
        }

        return schedule;
    }
}
