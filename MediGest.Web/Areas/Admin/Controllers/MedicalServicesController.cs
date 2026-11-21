
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Identity;
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.MedicalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = ApplicationRoles.Administrator)]
public class MedicalServicesController : Controller
{
    private readonly IMedicalService _medicalService;
    private readonly ISpecialtyService _specialtyService;

    public MedicalServicesController(
        IMedicalService medicalService,
        ISpecialtyService specialtyService)
    {
        _medicalService = medicalService;
        _specialtyService = specialtyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var medicalServices = (await _medicalService.GetAllAsync())
            .ToList();

        var viewModel = new MedicalServiceListViewModel
        {
            MedicalServices = medicalServices,
            TotalMedicalServices = medicalServices.Count,
            ActiveMedicalServices = medicalServices.Count(s => s.IsActive),
            InactiveMedicalServices = medicalServices.Count(s => !s.IsActive)
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new MedicalServiceFormViewModel();

        await LoadSpecialtiesAsync(viewModel);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        MedicalServiceFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await LoadSpecialtiesAsync(viewModel);
            return View(viewModel);
        }

        var medicalService = new MedicalService
        {
            Code = viewModel.Code,
            Name = viewModel.Name,
            Description = viewModel.Description,
            SpecialtyId = viewModel.SpecialtyId,
            PrivatePrice = viewModel.PrivatePrice
        };

        try
        {
            await _medicalService.CreateAsync(medicalService);

            TempData["Success"] =
                "Prestación médica registrada correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            await LoadSpecialtiesAsync(viewModel);

            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var medicalService = await _medicalService.GetByIdAsync(id);

        if (medicalService is null)
        {
            return NotFound();
        }

        var viewModel = new MedicalServiceFormViewModel
        {
            Id = medicalService.Id,
            Code = medicalService.Code,
            Name = medicalService.Name,
            Description = medicalService.Description,
            SpecialtyId = medicalService.SpecialtyId,
            PrivatePrice = medicalService.PrivatePrice
        };

        await LoadSpecialtiesAsync(viewModel);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        MedicalServiceFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await LoadSpecialtiesAsync(viewModel);
            return View(viewModel);
        }

        var medicalService = new MedicalService
        {
            Id = viewModel.Id,
            Code = viewModel.Code,
            Name = viewModel.Name,
            Description = viewModel.Description,
            SpecialtyId = viewModel.SpecialtyId,
            PrivatePrice = viewModel.PrivatePrice
        };

        try
        {
            await _medicalService.UpdateAsync(id, medicalService);

            TempData["Success"] =
                "Prestación médica actualizada correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            await LoadSpecialtiesAsync(viewModel);

            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await _medicalService.DeactivateAsync(id);

            TempData["Success"] =
                "Prestación médica desactivada correctamente.";
        }
        catch (BusinessException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            await _medicalService.ActivateAsync(id);

            TempData["Success"] =
                "Prestación médica activada correctamente.";
        }
        catch (BusinessException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSpecialtiesAsync(
        MedicalServiceFormViewModel viewModel)
    {
        var specialties = (await _specialtyService.GetActiveAsync())
            .ToList();

        // Al editar, conservar la especialidad actual aunque esté inactiva.
        if (viewModel.SpecialtyId.HasValue &&
            specialties.All(s => s.Id != viewModel.SpecialtyId.Value))
        {
            var currentSpecialty = await _specialtyService
                .GetByIdAsync(viewModel.SpecialtyId.Value);

            if (currentSpecialty is not null)
            {
                specialties.Add(currentSpecialty);
            }
        }

        viewModel.Specialties = specialties
            .OrderBy(s => s.Name)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.IsActive
                    ? s.Name
                    : $"{s.Name} (Inactiva)"
            })
            .ToList();
    }
}
