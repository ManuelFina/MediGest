
using System.Security.Claims;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Enums;
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.Appointments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.Areas.Reception.Controllers;

[Area("Reception")]
[Authorize(Roles = "Receptionist")]
public class AppointmentsController : Controller
{
    private readonly IAppointmentService _appointmentService;
    private readonly IAppointmentAvailabilityService _availabilityService;
    private readonly IPatientService _patientService;
    private readonly IProfessionalService _professionalService;

    public AppointmentsController(
        IAppointmentService appointmentService,
        IAppointmentAvailabilityService availabilityService,
        IPatientService patientService,
        IProfessionalService professionalService)
    {
        _appointmentService = appointmentService;
        _availabilityService = availabilityService;
        _patientService = patientService;
        _professionalService = professionalService;
    }

    // GET: Reception/Appointments
    [HttpGet]
    public async Task<IActionResult> Index(DateOnly? date)
    {
        var selectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);

        var daysSinceMonday =
            ((int)selectedDate.DayOfWeek + 6) % 7;

        var weekStart = selectedDate.AddDays(-daysSinceMonday);
        var weekEnd = weekStart.AddDays(6);

        var weekDays = Enumerable.Range(0, 7)
            .Select(offset => weekStart.AddDays(offset))
            .ToList();

        var appointments = new List<Appointment>();

        foreach (var day in weekDays)
        {
            var dailyAppointments = await _appointmentService
                .GetByDateAsync(day);

            appointments.AddRange(dailyAppointments);
        }

        var professionals = (await _professionalService.GetAllAsync())
            .Where(p => p.IsActive)
            .ToList();

        var viewModel = new AppointmentCalendarViewModel
        {
            SelectedDate = selectedDate,
            WeekStartDate = weekStart,
            WeekEndDate = weekEnd,
            WeekDays = weekDays,

            Appointments = appointments
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToList(),

            Professionals = professionals,

            TotalAppointments = appointments.Count,

            PendingAppointments = appointments.Count(
                a => a.Status == AppointmentStatus.Pending),

            ConfirmedAppointments = appointments.Count(
                a => a.Status == AppointmentStatus.Confirmed),

            CancelledAppointments = appointments.Count(
                a => a.Status == AppointmentStatus.Cancelled)
        };

        return View(viewModel);
    }

    // GET: Reception/Appointments/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = new AppointmentFormViewModel
        {
            AppointmentDate = DateOnly.FromDateTime(DateTime.Today),
            Status = AppointmentStatus.Pending
        };

        await LoadFormDataAsync(viewModel);

        return View(viewModel);
    }

    // POST: Reception/Appointments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AppointmentFormViewModel viewModel)
    {
        if (viewModel.Status != AppointmentStatus.Pending &&
            viewModel.Status != AppointmentStatus.Confirmed)
        {
            ModelState.AddModelError(
                nameof(viewModel.Status),
                "El estado inicial solo puede ser pendiente o confirmado.");
        }

        if (!ModelState.IsValid)
        {
            await LoadFormDataAsync(viewModel);
            return View(viewModel);
        }

        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Forbid();
        }

        try
        {
            var appointment = new Appointment
            {
                PatientId = viewModel.PatientId,
                ProfessionalId = viewModel.ProfessionalId,
                AppointmentDate = viewModel.AppointmentDate,
                StartTime = viewModel.StartTime,
                Status = viewModel.Status,
                Notes = viewModel.Notes
            };

            await _appointmentService.CreateAsync(
                appointment,
                userId);

            TempData["Success"] =
                "Turno asignado correctamente.";

            return RedirectToAction(
                nameof(Index),
                new { date = viewModel.AppointmentDate });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            await LoadFormDataAsync(viewModel);

            return View(viewModel);
        }
    }

    // GET: Reception/Appointments/GetAvailableSlots
    [HttpGet]
    public async Task<IActionResult> GetAvailableSlots(
        int professionalId,
        DateOnly date)
    {
        if (professionalId <= 0)
        {
            return BadRequest();
        }

        var slots = await _availabilityService.GetAvailableSlotsAsync(
            professionalId,
            date);

        return Json(slots.Select(slot => new
        {
            value = slot.ToString("HH:mm"),
            text = slot.ToString("HH:mm")
        }));
    }

    // GET: Reception/Appointments/History/5
    [HttpGet]
    public async Task<IActionResult> History(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);

        if (appointment is null)
        {
            return NotFound();
        }

        var viewModel = new AppointmentHistoryViewModel
        {
            AppointmentId = appointment.Id,

            PatientName =
                $"{appointment.Patient.LastName}, " +
                $"{appointment.Patient.FirstName}",

            ProfessionalName =
                $"{appointment.Professional.ApplicationUser.LastName}, " +
                $"{appointment.Professional.ApplicationUser.FirstName}",

            AppointmentDate = appointment.AppointmentDate,

            History = appointment.History
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new AppointmentHistoryItemViewModel
                {
                    Description = h.Description,
                    ChangedAt = h.ChangedAt,
                    ChangedByUserName =
                        h.ChangedByUser.UserName ?? "Usuario desconocido"
                })
                .ToList()
        };

        return View(viewModel);
    }

    // POST: Reception/Appointments/Confirm/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Confirm(int id)
    {
        return ExecuteAppointmentActionAsync(
            () => _appointmentService.ConfirmAsync(
                id,
                GetCurrentUserId()),
            "Turno confirmado correctamente.");
    }

    // POST: Reception/Appointments/MarkAsWaiting/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> MarkAsWaiting(int id)
    {
        return ExecuteAppointmentActionAsync(
            () => _appointmentService.MarkAsWaitingAsync(
                id,
                GetCurrentUserId()),
            "Llegada del paciente registrada correctamente.");
    }

    // POST: Reception/Appointments/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Cancel(int id, string reason)
    {
        return ExecuteAppointmentActionAsync(
            () => _appointmentService.CancelAsync(
                id,
                reason,
                GetCurrentUserId()),
            "Turno cancelado correctamente.");
    }

    // GET: Reception/Appointments/Reschedule/5
    [HttpGet]
    public async Task<IActionResult> Reschedule(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);

        if (appointment is null)
        {
            return NotFound();
        }

        if (!CanReschedule(appointment))
        {
            TempData["Error"] =
                "Solo se pueden reprogramar turnos pendientes o confirmados.";

            return RedirectToAction(nameof(Index));
        }

        var viewModel = new RescheduleAppointmentViewModel
        {
            AppointmentId = appointment.Id,
            ProfessionalId = appointment.ProfessionalId,

            PatientName =
                $"{appointment.Patient.LastName}, " +
                $"{appointment.Patient.FirstName}",

            ProfessionalName =
                $"{appointment.Professional.ApplicationUser.LastName}, " +
                $"{appointment.Professional.ApplicationUser.FirstName}",

            NewDate = appointment.AppointmentDate,
            NewStartTime = appointment.StartTime
        };

        await LoadRescheduleSlotsAsync(viewModel);

        return View(viewModel);
    }

    // POST: Reception/Appointments/Reschedule
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reschedule(
        RescheduleAppointmentViewModel viewModel)
    {
        var appointment = await _appointmentService
            .GetByIdAsync(viewModel.AppointmentId);

        if (appointment is null)
        {
            return NotFound();
        }

        if (!CanReschedule(appointment))
        {
            TempData["Error"] =
                "El estado actual del turno no permite reprogramarlo.";

            return RedirectToAction(nameof(Index));
        }

        viewModel.ProfessionalId = appointment.ProfessionalId;

        viewModel.PatientName =
            $"{appointment.Patient.LastName}, " +
            $"{appointment.Patient.FirstName}";

        viewModel.ProfessionalName =
            $"{appointment.Professional.ApplicationUser.LastName}, " +
            $"{appointment.Professional.ApplicationUser.FirstName}";

        if (!ModelState.IsValid)
        {
            await LoadRescheduleSlotsAsync(viewModel);
            return View(viewModel);
        }

        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Forbid();
        }

        try
        {
            await _appointmentService.RescheduleAsync(
                viewModel.AppointmentId,
                viewModel.NewDate,
                viewModel.NewStartTime,
                userId);

            TempData["Success"] =
                "Turno reprogramado correctamente.";

            return RedirectToAction(
                nameof(Index),
                new { date = viewModel.NewDate });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(
                string.Empty,
                exception.Message);

            await LoadRescheduleSlotsAsync(viewModel);

            return View(viewModel);
        }
    }

    // GET: Reception/Appointments/GetRescheduleSlots
    [HttpGet]
    public async Task<IActionResult> GetRescheduleSlots(
        int appointmentId,
        DateOnly date)
    {
        var appointment = await _appointmentService
            .GetByIdAsync(appointmentId);

        if (appointment is null)
        {
            return NotFound();
        }

        if (!CanReschedule(appointment))
        {
            return BadRequest();
        }

        var slots = await _availabilityService.GetAvailableSlotsAsync(
            appointment.ProfessionalId,
            date,
            appointment.Id);

        return Json(slots.Select(slot => new
        {
            value = slot.ToString("HH:mm"),
            text = slot.ToString("HH:mm")
        }));
    }

    // POST: Reception/Appointments/MarkAsNoShow/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> MarkAsNoShow(int id)
    {
        return ExecuteAppointmentActionAsync(
            () => _appointmentService.MarkAsNoShowAsync(
                id,
                GetCurrentUserId()),
            "Turno marcado como ausente.");
    }

    // Helpers

    private async Task LoadFormDataAsync(
        AppointmentFormViewModel viewModel)
    {
        var patients = await _patientService.GetAllAsync();

        var professionals = await _professionalService.GetAllAsync();

        viewModel.AvailablePatients = patients
            .Where(p => p.IsActive)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.LastName}, {p.FirstName}"
            })
            .ToList();

        viewModel.AvailableProfessionals = professionals
            .Where(p => p.IsActive)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text =
                    $"{p.ApplicationUser.LastName}, " +
                    $"{p.ApplicationUser.FirstName}"
            })
            .ToList();

        viewModel.AvailableSlots = new List<SelectListItem>();

        if (viewModel.ProfessionalId > 0 &&
            viewModel.AppointmentDate != default)
        {
            var slots = await _availabilityService.GetAvailableSlotsAsync(
                viewModel.ProfessionalId,
                viewModel.AppointmentDate);

            viewModel.AvailableSlots = slots
                .Select(slot => new SelectListItem
                {
                    Value = slot.ToString("HH:mm"),
                    Text = slot.ToString("HH:mm")
                })
                .ToList();
        }
    }

    private async Task LoadRescheduleSlotsAsync(
        RescheduleAppointmentViewModel viewModel)
    {
        viewModel.AvailableSlots = new List<SelectListItem>();

        if (viewModel.ProfessionalId <= 0 ||
            viewModel.NewDate == default)
        {
            return;
        }

        var slots = await _availabilityService.GetAvailableSlotsAsync(
            viewModel.ProfessionalId,
            viewModel.NewDate,
            viewModel.AppointmentId);

        viewModel.AvailableSlots = slots
            .Select(slot => new SelectListItem
            {
                Value = slot.ToString("HH:mm"),
                Text = slot.ToString("HH:mm")
            })
            .ToList();
    }

    private async Task<IActionResult> ExecuteAppointmentActionAsync(
        Func<Task> action,
        string successMessage)
    {
        var userId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Forbid();
        }

        try
        {
            await action();

            TempData["Success"] = successMessage;
        }
        catch (BusinessException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? string.Empty;
    }

    private static bool CanReschedule(Appointment appointment)
    {
        return appointment.Status is
            AppointmentStatus.Pending or
            AppointmentStatus.Confirmed;
    }
}
