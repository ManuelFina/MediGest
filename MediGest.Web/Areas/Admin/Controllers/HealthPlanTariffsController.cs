
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Identity;
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.HealthPlanTariffs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = ApplicationRoles.Administrator)]
public class HealthPlanTariffsController : Controller
{
    private const int PageSize = 25;

    private readonly IHealthPlanTariffService _tariffService;
    private readonly IHealthPlanService _healthPlanService;
    private readonly IMedicalService _medicalService;

    public HealthPlanTariffsController(
        IHealthPlanTariffService tariffService,
        IHealthPlanService healthPlanService,
        IMedicalService medicalService)
    {
        _tariffService = tariffService;
        _healthPlanService = healthPlanService;
        _medicalService = medicalService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int page = 1)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var tariffs = await _tariffService.GetPageAsync(page, PageSize);
        var statistics = await _tariffService.GetStatisticsAsync(today);

        var viewModel = new HealthPlanTariffListViewModel
        {
            Tariffs = tariffs.Tariffs,
            TotalTariffs = statistics.TotalTariffs,
            CurrentTariffs = statistics.CurrentTariffs,
            ExpiredTariffs = statistics.ExpiredTariffs,
            FutureTariffs = statistics.FutureTariffs
        };

        ViewBag.Pagination = tariffs;

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        string? searchTerm,
        int page = 1)
    {
        var tariffs = await _tariffService.SearchAsync(
            searchTerm,
            page,
            PageSize);

        Response.Headers["X-Results-Count"] =
            tariffs.TotalCount.ToString();

        Response.Headers["X-Current-Page"] =
            tariffs.Page.ToString();

        Response.Headers["X-Total-Pages"] =
            tariffs.TotalPages.ToString();

        return PartialView("_HealthPlanTariffRows", tariffs.Tariffs);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new HealthPlanTariffFormViewModel();

        await LoadSelectListsAsync(viewModel);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        HealthPlanTariffFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await LoadSelectListsAsync(viewModel);
            return View(viewModel);
        }

        var tariff = new HealthPlanTariff
        {
            HealthPlanId = viewModel.HealthPlanId,
            MedicalServiceId = viewModel.MedicalServiceId,
            Amount = viewModel.Amount,
            ValidFrom = viewModel.ValidFrom,
            ValidTo = viewModel.ValidTo,
            RequiresAuthorization = viewModel.RequiresAuthorization,
            Notes = viewModel.Notes
        };

        try
        {
            await _tariffService.CreateAsync(tariff);

            TempData["Success"] =
                "Tarifa registrada correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            await LoadSelectListsAsync(viewModel);

            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id, DateOnly validTo)
    {
        try
        {
            await _tariffService.CloseAsync(id, validTo);

            TempData["Success"] =
                "La vigencia de la tarifa se cerró correctamente.";
        }
        catch (BusinessException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadSelectListsAsync(
        HealthPlanTariffFormViewModel viewModel)
    {
        var healthPlans = await _healthPlanService.GetActiveAsync();

        viewModel.HealthPlans = healthPlans
            .OrderBy(p => p.HealthInsurance.Name)
            .ThenBy(p => p.Name)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.HealthInsurance.Name} - {p.Name}"
            })
            .ToList();

        var medicalServices = await _medicalService.GetActiveAsync();

        viewModel.MedicalServices = medicalServices
            .OrderBy(s => s.Name)
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(s.Code)
                    ? s.Name
                    : $"{s.Code} - {s.Name}"
            })
            .ToList();
    }
}
