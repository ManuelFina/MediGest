
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.Patients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediGest.Web.Areas.Reception.Controllers;

[Area("Reception")]
[Authorize(Roles = "Administrator,Receptionist")]
public class PatientsController : Controller
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var allPatients = await _patientService.GetAllAsync();

        var patients = string.IsNullOrWhiteSpace(searchTerm)
            ? allPatients
            : await _patientService.SearchAsync(searchTerm);

        var viewModel = new PatientListViewModel
        {
            Patients = patients,
            SearchTerm = searchTerm,
            TotalPatients = allPatients.Count(),
            ActivePatients = allPatients.Count(p => p.IsActive),
            InactivePatients = allPatients.Count(p => !p.IsActive)
        };

        return View(viewModel);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new PatientFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PatientFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var patient = new Patient
        {
            FirstName = viewModel.FirstName,
            LastName = viewModel.LastName,
            DocumentNumber = viewModel.DocumentNumber,
            DateOfBirth = viewModel.DateOfBirth,
            PhoneNumber = viewModel.PhoneNumber,
            Email = viewModel.Email,
            Address = viewModel.Address
        };

        try
        {
            await _patientService.CreateAsync(patient);

            TempData["Success"] = "Paciente registrado correctamente.";

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
        var patient = await _patientService.GetByIdAsync(id);

        if (patient is null)
        {
            return NotFound();
        }

        var viewModel = new PatientFormViewModel
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            DocumentNumber = patient.DocumentNumber,
            DateOfBirth = patient.DateOfBirth,
            PhoneNumber = patient.PhoneNumber,
            Email = patient.Email,
            Address = patient.Address
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        PatientFormViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var patient = new Patient
        {
            Id = viewModel.Id,
            FirstName = viewModel.FirstName,
            LastName = viewModel.LastName,
            DocumentNumber = viewModel.DocumentNumber,
            DateOfBirth = viewModel.DateOfBirth,
            PhoneNumber = viewModel.PhoneNumber,
            Email = viewModel.Email,
            Address = viewModel.Address
        };

        try
        {
            await _patientService.UpdateAsync(patient);

            TempData["Success"] = "Paciente actualizado correctamente.";

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
            await _patientService.DeactivateAsync(id);

            TempData["Success"] = "Paciente desactivado correctamente.";
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
            await _patientService.ActivateAsync(id);

            TempData["Success"] = "Paciente activado correctamente.";
        }
        catch (BusinessException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
