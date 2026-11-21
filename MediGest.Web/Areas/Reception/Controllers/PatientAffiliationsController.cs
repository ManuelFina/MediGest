
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Enums;
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.PatientAffiliations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.Areas.Reception.Controllers;

[Area("Reception")]
[Authorize(Roles = "Receptionist")]
[Route("Reception/Patients/{patientId:int}/Affiliations")]
public class PatientAffiliationsController : Controller
{
    private readonly IPatientAffiliationService _affiliationService;
    private readonly IPatientService _patientService;
    private readonly IHealthInsuranceService _healthInsuranceService;
    private readonly IHealthPlanService _healthPlanService;

    public PatientAffiliationsController(
        IPatientAffiliationService affiliationService,
        IPatientService patientService,
        IHealthInsuranceService healthInsuranceService,
        IHealthPlanService healthPlanService)
    {
        _affiliationService = affiliationService;
        _patientService = patientService;
        _healthInsuranceService = healthInsuranceService;
        _healthPlanService = healthPlanService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int patientId)
    {
        var patient = await _patientService.GetByIdAsync(patientId);

        if (patient is null)
            return NotFound();

        var affiliations = await _affiliationService
            .GetByPatientIdAsync(patientId);

        var viewModel = new PatientAffiliationListViewModel
        {
            PatientId = patient.Id,
            PatientName = $"{patient.FirstName} {patient.LastName}",
            DocumentNumber = patient.DocumentNumber,
            Affiliations = affiliations
        };

        return View(viewModel);
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create(int patientId)
    {
        var patient = await _patientService.GetByIdAsync(patientId);

        if (patient is null)
            return NotFound();

        if (!patient.IsActive)
        {
            TempData["Error"] =
                "No se pueden registrar afiliaciones para un paciente desactivado.";

            return RedirectToAction(nameof(Index), new { patientId });
        }

        var viewModel = new PatientAffiliationFormViewModel
        {
            PatientId = patient.Id,
            PatientName = $"{patient.FirstName} {patient.LastName}",
            ValidFrom = DateOnly.FromDateTime(DateTime.Today)
        };

        await LoadFormDataAsync(viewModel);

        return View(viewModel);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        int patientId,
        PatientAffiliationFormViewModel viewModel)
    {
        var patient = await _patientService.GetByIdAsync(patientId);

        if (patient is null)
            return NotFound();

        if (!patient.IsActive)
        {
            TempData["Error"] =
                "No se pueden registrar afiliaciones para un paciente desactivado.";

            return RedirectToAction(nameof(Index), new { patientId });
        }

        viewModel.PatientId = patientId;
        viewModel.PatientName = $"{patient.FirstName} {patient.LastName}";

        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync(viewModel);
            return View(viewModel);
        }

        var affiliation = new PatientAffiliation
        {
            PatientId = patientId,
            HealthInsuranceId = viewModel.HealthInsuranceId,
            HealthPlanId = viewModel.HealthPlanId,
            MembershipNumber = viewModel.MembershipNumber,
            MemberType = viewModel.MemberType,
            ValidFrom = viewModel.ValidFrom,
            ValidTo = viewModel.ValidTo,
            IsPrimary = viewModel.IsPrimary,
            Notes = viewModel.Notes,
            Status = PatientAffiliationStatus.Active
        };

        try
        {
            await _affiliationService.CreateAsync(affiliation);

            TempData["Success"] =
                "Afiliación registrada correctamente.";

            return RedirectToAction(nameof(Index), new { patientId });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            await LoadFormDataAsync(viewModel);

            return View(viewModel);
        }
    }

    [HttpGet("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int patientId, int id)
    {
        var affiliation = await _affiliationService.GetByIdAsync(id);

        if (affiliation is null || affiliation.PatientId != patientId)
            return NotFound();

        if (affiliation.Status == PatientAffiliationStatus.Cancelled)
        {
            TempData["Error"] =
                "No se puede editar una afiliación cancelada.";

            return RedirectToAction(nameof(Index), new { patientId });
        }

        var viewModel = new PatientAffiliationFormViewModel
        {
            Id = affiliation.Id,
            PatientId = affiliation.PatientId,
            PatientName = $"{affiliation.Patient.FirstName} {affiliation.Patient.LastName}",
            HealthInsuranceId = affiliation.HealthInsuranceId,
            HealthPlanId = affiliation.HealthPlanId,
            MembershipNumber = affiliation.MembershipNumber,
            MemberType = affiliation.MemberType,
            ValidFrom = affiliation.ValidFrom,
            ValidTo = affiliation.ValidTo,
            IsPrimary = affiliation.IsPrimary,
            Notes = affiliation.Notes
        };

        await LoadFormDataAsync(viewModel);

        return View(viewModel);
    }

    [HttpPost("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int patientId,
        int id,
        PatientAffiliationFormViewModel viewModel)
    {
        if (id != viewModel.Id)
            return BadRequest();

        var existingAffiliation = await _affiliationService.GetByIdAsync(id);

        if (existingAffiliation is null ||
            existingAffiliation.PatientId != patientId)
        {
            return NotFound();
        }

        if (existingAffiliation.Status == PatientAffiliationStatus.Cancelled)
        {
            TempData["Error"] =
                "No se puede editar una afiliación cancelada.";

            return RedirectToAction(nameof(Index), new { patientId });
        }

        viewModel.PatientId = patientId;
        viewModel.PatientName =
            $"{existingAffiliation.Patient.FirstName} {existingAffiliation.Patient.LastName}";

        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync(viewModel);
            return View(viewModel);
        }

        var affiliation = new PatientAffiliation
        {
            Id = id,
            PatientId = patientId,
            HealthInsuranceId = viewModel.HealthInsuranceId,
            HealthPlanId = viewModel.HealthPlanId,
            MembershipNumber = viewModel.MembershipNumber,
            MemberType = viewModel.MemberType,
            ValidFrom = viewModel.ValidFrom,
            ValidTo = viewModel.ValidTo,
            IsPrimary = viewModel.IsPrimary,
            Notes = viewModel.Notes
        };

        try
        {
            await _affiliationService.UpdateAsync(id, affiliation);

            TempData["Success"] =
                "Afiliación actualizada correctamente.";

            return RedirectToAction(nameof(Index), new { patientId });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            await LoadFormDataAsync(viewModel);

            return View(viewModel);
        }
    }

    [HttpPost("{id:int}/Suspend")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(int patientId, int id)
    {
        var affiliation = await _affiliationService.GetByIdAsync(id);

        if (affiliation is null || affiliation.PatientId != patientId)
        {
            return NotFound();
        }

        try
        {
            await _affiliationService.SuspendAsync(id);

            TempData["Success"] = "Afiliación suspendida correctamente.";
        }
        catch (BusinessException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { patientId });
    }

    [HttpPost("{id:int}/Reactivate")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(int patientId, int id)
    {
        var affiliation = await _affiliationService.GetByIdAsync(id);

        if (affiliation is null || affiliation.PatientId != patientId)
        {
            return NotFound();
        }

        try
        {
            await _affiliationService.ReactivateAsync(id);

            TempData["Success"] = "Afiliación reactivada correctamente.";
        }
        catch (BusinessException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { patientId });
    }

    [HttpPost("{id:int}/Cancel")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int patientId, int id)
    {
        var affiliation = await _affiliationService.GetByIdAsync(id);

        if (affiliation is null || affiliation.PatientId != patientId)
        {
            return NotFound();
        }

        try
        {
            await _affiliationService.CancelAsync(id);

            TempData["Success"] = "Afiliación cancelada correctamente.";
        }
        catch (BusinessException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index), new { patientId });
    }


    [HttpGet("Plans")]
    public async Task<IActionResult> GetPlans(int patientId, int healthInsuranceId)
    {
        var patient = await _patientService.GetByIdAsync(patientId);

        if (patient is null)
            return NotFound();

        var plans = await _healthPlanService.GetActiveAsync();

        var result = plans
            .Where(p => p.HealthInsuranceId == healthInsuranceId)
            .Select(p => new
            {
                id = p.Id,
                name = p.Name
            });

        return Json(result);
    }


    private async Task LoadFormDataAsync(
        PatientAffiliationFormViewModel viewModel)
    {
        var healthInsurances = await _healthInsuranceService.GetActiveAsync();

        viewModel.HealthInsurances = healthInsurances
            .Select(insurance => new SelectListItem
            {
                Value = insurance.Id.ToString(),
                Text = insurance.Name,
                Selected = insurance.Id == viewModel.HealthInsuranceId
            })
            .ToList();

        var healthPlans = await _healthPlanService.GetActiveAsync();

        viewModel.HealthPlans = healthPlans
            .Where(plan =>
                plan.HealthInsuranceId == viewModel.HealthInsuranceId)
            .Select(plan => new SelectListItem
            {
                Value = plan.Id.ToString(),
                Text = plan.Name,
                Selected = plan.Id == viewModel.HealthPlanId
            })
            .ToList();
    }
}
