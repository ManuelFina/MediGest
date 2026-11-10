
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Identity;
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.Professionals;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = ApplicationRoles.Administrator)]
public class ProfessionalsController : Controller
{
    private readonly IProfessionalService _professionalService;
    private readonly ISpecialtyService _specialtyService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfessionalsController(
        IProfessionalService professionalService,
        ISpecialtyService specialtyService,
        UserManager<ApplicationUser> userManager)
    {
        _professionalService = professionalService;
        _specialtyService = specialtyService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var allProfessionals = await _professionalService.GetAllAsync();

        var professionals = string.IsNullOrWhiteSpace(searchTerm)
            ? allProfessionals
            : await _professionalService.SearchAsync(searchTerm);

        var viewModel = new ProfessionalListViewModel
        {
            Professionals = professionals,
            SearchTerm = searchTerm,
            TotalProfessionals = allProfessionals.Count(),
            ActiveProfessionals = allProfessionals.Count(p => p.IsActive),
            InactiveProfessionals = allProfessionals.Count(p => !p.IsActive)
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new ProfessionalFormViewModel();

        await LoadFormDataAsync(viewModel);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ProfessionalFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync(viewModel);
            return View(viewModel);
        }

        var professional = new Professional
        {
            ApplicationUserId = viewModel.ApplicationUserId,
            LicenseNumber = viewModel.LicenseNumber,
            SpecialtyId = viewModel.SpecialtyId
        };

        try
        {
            await _professionalService.CreateAsync(professional);

            TempData["Success"] =
                "Profesional registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            await LoadFormDataAsync(viewModel);

            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var professional = await _professionalService.GetByIdAsync(id);

        if (professional is null)
        {
            return NotFound();
        }

        var viewModel = new ProfessionalFormViewModel
        {
            Id = professional.Id,
            ApplicationUserId = professional.ApplicationUserId,
            LicenseNumber = professional.LicenseNumber,
            SpecialtyId = professional.SpecialtyId,
            IsActive = professional.IsActive
        };

        await LoadFormDataAsync(viewModel);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        ProfessionalFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync(viewModel);
            return View(viewModel);
        }

        var professional = new Professional
        {
            Id = viewModel.Id,
            ApplicationUserId = viewModel.ApplicationUserId,
            LicenseNumber = viewModel.LicenseNumber,
            SpecialtyId = viewModel.SpecialtyId
        };

        try
        {
            await _professionalService.UpdateAsync(professional);

            TempData["Success"] =
                "Profesional actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            await LoadFormDataAsync(viewModel);

            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await _professionalService.DeactivateAsync(id);

            TempData["Success"] =
                "Profesional desactivado correctamente.";
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
            await _professionalService.ActivateAsync(id);

            TempData["Success"] =
                "Profesional activado correctamente.";
        }
        catch (BusinessException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadFormDataAsync(
        ProfessionalFormViewModel viewModel)
    {
        await LoadAvailableUsersAsync(viewModel);
        await LoadAvailableSpecialtiesAsync(viewModel);
    }

    private async Task LoadAvailableUsersAsync(
        ProfessionalFormViewModel viewModel)
    {
        var users = await _userManager.GetUsersInRoleAsync(
            ApplicationRoles.Doctor);

        var professionals = await _professionalService.GetAllAsync();

        var assignedIds = professionals
            .Select(p => p.ApplicationUserId)
            .ToHashSet();

        viewModel.AvailableUsers = users
            .Where(u =>
                !assignedIds.Contains(u.Id) ||
                u.Id == viewModel.ApplicationUserId)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = $"{u.LastName}, {u.FirstName} ({u.Email})"
            })
            .ToList();
    }

    private async Task LoadAvailableSpecialtiesAsync(
        ProfessionalFormViewModel viewModel)
    {
        var specialties = (await _specialtyService.GetActiveAsync())
            .ToList();

        // Include the current specialty when editing an inactive professional.
        if (viewModel.SpecialtyId > 0 &&
            specialties.All(s => s.Id != viewModel.SpecialtyId))
        {
            var currentSpecialty = await _specialtyService
                .GetByIdAsync(viewModel.SpecialtyId);

            if (currentSpecialty is not null)
            {
                specialties.Add(currentSpecialty);
            }
        }

        viewModel.AvailableSpecialties = specialties
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
