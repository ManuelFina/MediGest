
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Identity;
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.HealthPlans;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = ApplicationRoles.Administrator)]
public class HealthPlansController : Controller
{
    private readonly IHealthPlanService _healthPlanService;
    private readonly IHealthInsuranceService _healthInsuranceService;

    public HealthPlansController(
        IHealthPlanService healthPlanService,
        IHealthInsuranceService healthInsuranceService)
    {
        _healthPlanService = healthPlanService;
        _healthInsuranceService = healthInsuranceService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var healthPlans = (await _healthPlanService.GetAllAsync())
            .ToList();

        var viewModel = new HealthPlanListViewModel
        {
            HealthPlans = healthPlans,
            TotalHealthPlans = healthPlans.Count,
            ActiveHealthPlans = healthPlans.Count(p => p.IsActive),
            InactiveHealthPlans = healthPlans.Count(p => !p.IsActive)
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new HealthPlanFormViewModel();

        await LoadHealthInsurancesAsync(viewModel);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        HealthPlanFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await LoadHealthInsurancesAsync(viewModel);
            return View(viewModel);
        }

        var healthPlan = new HealthPlan
        {
            HealthInsuranceId = viewModel.HealthInsuranceId,
            Name = viewModel.Name,
            Code = viewModel.Code,
            Description = viewModel.Description,
            RequiresAuthorization = viewModel.RequiresAuthorization
        };

        try
        {
            await _healthPlanService.CreateAsync(healthPlan);

            TempData["Success"] =
                "Plan médico registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            await LoadHealthInsurancesAsync(viewModel);

            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var healthPlan = await _healthPlanService.GetByIdAsync(id);

        if (healthPlan is null)
        {
            return NotFound();
        }

        var viewModel = new HealthPlanFormViewModel
        {
            Id = healthPlan.Id,
            HealthInsuranceId = healthPlan.HealthInsuranceId,
            Name = healthPlan.Name,
            Code = healthPlan.Code,
            Description = healthPlan.Description,
            RequiresAuthorization = healthPlan.RequiresAuthorization
        };

        await LoadHealthInsurancesAsync(viewModel);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        HealthPlanFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await LoadHealthInsurancesAsync(viewModel);
            return View(viewModel);
        }

        var healthPlan = new HealthPlan
        {
            Id = viewModel.Id,
            HealthInsuranceId = viewModel.HealthInsuranceId,
            Name = viewModel.Name,
            Code = viewModel.Code,
            Description = viewModel.Description,
            RequiresAuthorization = viewModel.RequiresAuthorization
        };

        try
        {
            await _healthPlanService.UpdateAsync(id, healthPlan);

            TempData["Success"] =
                "Plan médico actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            await LoadHealthInsurancesAsync(viewModel);

            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(int id)
    {
        try
        {
            await _healthPlanService.DeactivateAsync(id);

            TempData["Success"] =
                "Plan médico desactivado correctamente.";
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
            await _healthPlanService.ActivateAsync(id);

            TempData["Success"] =
                "Plan médico activado correctamente.";
        }
        catch (BusinessException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadHealthInsurancesAsync(
        HealthPlanFormViewModel viewModel)
    {
        var healthInsurances = (await _healthInsuranceService.GetActiveAsync())
            .ToList();

        // Al editar, conservar la cobertura actual aunque esté inactiva.
        if (viewModel.HealthInsuranceId > 0 &&
            healthInsurances.All(h => h.Id != viewModel.HealthInsuranceId))
        {
            var currentHealthInsurance = await _healthInsuranceService
                .GetByIdAsync(viewModel.HealthInsuranceId);

            if (currentHealthInsurance is not null)
            {
                healthInsurances.Add(currentHealthInsurance);
            }
        }

        viewModel.HealthInsurances = healthInsurances
            .OrderBy(h => h.Name)
            .Select(h => new SelectListItem
            {
                Value = h.Id.ToString(),
                Text = h.IsActive
                    ? h.Name
                    : $"{h.Name} (Inactiva)"
            })
            .ToList();
    }
}
