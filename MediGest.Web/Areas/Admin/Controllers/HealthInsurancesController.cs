
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Enums;
using MediGest.Web.Models.Identity;
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.HealthInsurances;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediGest.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = ApplicationRoles.Administrator)]
public class HealthInsurancesController : Controller
{
    private readonly IHealthInsuranceService _healthInsuranceService;

    public HealthInsurancesController(
        IHealthInsuranceService healthInsuranceService)
    {
        _healthInsuranceService = healthInsuranceService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var healthInsurances = (await _healthInsuranceService.GetAllAsync())
            .ToList();

        var viewModel = new HealthInsuranceListViewModel
        {
            HealthInsurances = healthInsurances,
            TotalHealthInsurances = healthInsurances.Count,
            ActiveHealthInsurances = healthInsurances.Count(h => h.IsActive),
            InactiveHealthInsurances = healthInsurances.Count(h => !h.IsActive)
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string? searchTerm)
    {
        var healthInsurances = (await _healthInsuranceService.GetAllAsync())
            .Where(insurance =>
                string.IsNullOrWhiteSpace(searchTerm) ||
                insurance.Name.Contains(
                    searchTerm, StringComparison.OrdinalIgnoreCase) ||
                (insurance.LegalName != null &&
                 insurance.LegalName.Contains(
                    searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (insurance.CUIT != null &&
                 insurance.CUIT.Contains(
                    searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                GetEntityTypeLabel(insurance.EntityType).Contains(
                    searchTerm, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Response.Headers["X-Results-Count"] =
            healthInsurances.Count.ToString();

        return PartialView("_HealthInsuranceRows", healthInsurances);
    }

    private static string GetEntityTypeLabel(HealthInsuranceType entityType)
    {
        return entityType switch
        {
            HealthInsuranceType.SocialSecurity => "Obra social",
            HealthInsuranceType.PrepaidMedicine => "Prepaga",
            _ => "Otra"
        };
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new HealthInsuranceFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        HealthInsuranceFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var healthInsurance = new HealthInsurance
        {
            Name = viewModel.Name,
            LegalName = viewModel.LegalName,
            CUIT = viewModel.CUIT,
            EntityType = viewModel.EntityType,
            RegistrationNumber = viewModel.RegistrationNumber,
            PhoneNumber = viewModel.PhoneNumber,
            Email = viewModel.Email,
            Address = viewModel.Address,
            Notes = viewModel.Notes
        };

        try
        {
            await _healthInsuranceService.CreateAsync(healthInsurance);

            TempData["Success"] =
                "Cobertura médica registrada correctamente.";

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
        var healthInsurance = await _healthInsuranceService.GetByIdAsync(id);

        if (healthInsurance is null)
        {
            return NotFound();
        }

        var viewModel = new HealthInsuranceFormViewModel
        {
            Id = healthInsurance.Id,
            Name = healthInsurance.Name,
            LegalName = healthInsurance.LegalName,
            CUIT = healthInsurance.CUIT,
            EntityType = healthInsurance.EntityType,
            RegistrationNumber = healthInsurance.RegistrationNumber,
            PhoneNumber = healthInsurance.PhoneNumber,
            Email = healthInsurance.Email,
            Address = healthInsurance.Address,
            Notes = healthInsurance.Notes
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        HealthInsuranceFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var healthInsurance = new HealthInsurance
        {
            Id = viewModel.Id,
            Name = viewModel.Name,
            LegalName = viewModel.LegalName,
            CUIT = viewModel.CUIT,
            EntityType = viewModel.EntityType,
            RegistrationNumber = viewModel.RegistrationNumber,
            PhoneNumber = viewModel.PhoneNumber,
            Email = viewModel.Email,
            Address = viewModel.Address,
            Notes = viewModel.Notes
        };

        try
        {
            await _healthInsuranceService.UpdateAsync(id, healthInsurance);

            TempData["Success"] =
                "Cobertura médica actualizada correctamente.";

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
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await _healthInsuranceService.DeactivateAsync(id);

            TempData["Success"] =
                "Cobertura médica desactivada correctamente.";
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
            await _healthInsuranceService.ActivateAsync(id);

            TempData["Success"] =
                "Cobertura médica activada correctamente.";
        }
        catch (BusinessException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
