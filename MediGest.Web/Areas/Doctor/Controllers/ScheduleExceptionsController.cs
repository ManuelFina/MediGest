
using System.Security.Claims;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Identity;
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.Schedules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediGest.Web.Areas.Doctor.Controllers;

[Area("Doctor")]
[Authorize(Roles = ApplicationRoles.Doctor)]
public class ScheduleExceptionsController : Controller
{
    private readonly IScheduleExceptionService _exceptionService;
    private readonly IProfessionalService _professionalService;

    public ScheduleExceptionsController(
        IScheduleExceptionService exceptionService,
        IProfessionalService professionalService)
    {
        _exceptionService = exceptionService;
        _professionalService = professionalService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var professional = await GetCurrentProfessionalAsync();

        if (professional is null)
            return Forbid();

        var exceptions = await _exceptionService
            .GetByProfessionalAsync(professional.Id);

        return View(exceptions);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var professional = await GetCurrentProfessionalAsync();

        if (professional is null)
            return Forbid();

        return View(new ScheduleExceptionFormViewModel
        {
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ScheduleExceptionFormViewModel viewModel)
    {
        var professional = await GetCurrentProfessionalAsync();

        if (professional is null)
            return Forbid();

        if (!ModelState.IsValid)
            return View(viewModel);

        var scheduleException = new ScheduleException
        {
            ProfessionalId = professional.Id,
            StartDate = viewModel.StartDate,
            EndDate = viewModel.EndDate,
            StartTime = viewModel.StartTime,
            EndTime = viewModel.EndTime,
            Reason = viewModel.Reason
        };

        try
        {
            await _exceptionService.CreateAsync(scheduleException);

            TempData["Success"] =
                "Excepción de agenda registrada correctamente.";

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
            return Forbid();

        var scheduleException = await _exceptionService.GetByIdAsync(id);

        if (scheduleException is null)
            return NotFound();

        if (scheduleException.ProfessionalId != professional.Id)
            return Forbid();

        var viewModel = new ScheduleExceptionFormViewModel
        {
            Id = scheduleException.Id,
            ProfessionalId = professional.Id,
            StartDate = scheduleException.StartDate,
            EndDate = scheduleException.EndDate,
            StartTime = scheduleException.StartTime,
            EndTime = scheduleException.EndTime,
            Reason = scheduleException.Reason
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        ScheduleExceptionFormViewModel viewModel)
    {
        if (id != viewModel.Id)
            return BadRequest();

        var professional = await GetCurrentProfessionalAsync();

        if (professional is null)
            return Forbid();

        var existingException = await _exceptionService.GetByIdAsync(id);

        if (existingException is null)
            return NotFound();

        if (existingException.ProfessionalId != professional.Id)
            return Forbid();

        if (!ModelState.IsValid)
        {
            viewModel.ProfessionalId = professional.Id;
            return View(viewModel);
        }

        var scheduleException = new ScheduleException
        {
            Id = id,
            ProfessionalId = professional.Id,
            StartDate = viewModel.StartDate,
            EndDate = viewModel.EndDate,
            StartTime = viewModel.StartTime,
            EndTime = viewModel.EndTime,
            Reason = viewModel.Reason
        };

        try
        {
            await _exceptionService.UpdateAsync(scheduleException);

            TempData["Success"] =
                "Excepción de agenda actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            viewModel.ProfessionalId = professional.Id;
            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var professional = await GetCurrentProfessionalAsync();

        if (professional is null)
            return Forbid();

        var scheduleException = await _exceptionService.GetByIdAsync(id);

        if (scheduleException is null)
            return NotFound();

        if (scheduleException.ProfessionalId != professional.Id)
            return Forbid();

        try
        {
            await _exceptionService.DeleteAsync(id);

            TempData["Success"] =
                "Excepción de agenda eliminada correctamente.";
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
            return null;

        return await _professionalService
            .GetByApplicationUserIdAsync(userId);
    }
}
