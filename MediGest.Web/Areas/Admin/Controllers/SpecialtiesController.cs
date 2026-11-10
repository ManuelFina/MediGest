
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Identity;
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.Specialties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediGest.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = ApplicationRoles.Administrator)]
public class SpecialtiesController : Controller
{
    private readonly ISpecialtyService _specialtyService;

    public SpecialtiesController(ISpecialtyService specialtyService)
    {
        _specialtyService = specialtyService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var allSpecialties = (await _specialtyService.GetAllAsync())
            .ToList();

        var filteredSpecialties = allSpecialties.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var normalizedSearch = searchTerm.Trim();

            filteredSpecialties = filteredSpecialties.Where(s =>
                s.Name.Contains(
                    normalizedSearch,
                    StringComparison.OrdinalIgnoreCase));
        }

        var viewModel = new SpecialtyListViewModel
        {
            SearchTerm = searchTerm,
            TotalSpecialties = allSpecialties.Count,
            ActiveSpecialties = allSpecialties.Count(s => s.IsActive),
            InactiveSpecialties = allSpecialties.Count(s => !s.IsActive),

            Specialties = filteredSpecialties
                .Select(s => new SpecialtyItemViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    ProfessionalsCount = s.Professionals.Count
                })
                .ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new SpecialtyFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        SpecialtyFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            var specialty = new Specialty
            {
                Name = viewModel.Name,
                Description = viewModel.Description
            };

            await _specialtyService.CreateAsync(specialty);

            TempData["Success"] =
                "La especialidad se creó correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var specialty = await _specialtyService.GetByIdAsync(id);

        if (specialty is null)
        {
            return NotFound();
        }

        var viewModel = new SpecialtyFormViewModel
        {
            Id = specialty.Id,
            Name = specialty.Name,
            Description = specialty.Description
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        SpecialtyFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        try
        {
            var specialty = new Specialty
            {
                Id = id,
                Name = viewModel.Name,
                Description = viewModel.Description
            };

            await _specialtyService.UpdateAsync(id, specialty);

            TempData["Success"] =
                "La especialidad se actualizó correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await _specialtyService.DeactivateAsync(id);

            TempData["Success"] =
                "La especialidad se desactivó correctamente.";
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            await _specialtyService.ActivateAsync(id);

            TempData["Success"] =
                "La especialidad se activó correctamente.";
        }
        catch (BusinessException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
