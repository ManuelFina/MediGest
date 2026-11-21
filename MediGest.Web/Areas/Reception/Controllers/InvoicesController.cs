
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Enums;
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.Invoices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.Areas.Reception.Controllers;

[Area("Reception")]
[Authorize(Roles = "Receptionist")]
public class InvoicesController : Controller
{
    private readonly IInvoiceService _invoiceService;
    private readonly IPaymentService _paymentService;
    private readonly IPatientService _patientService;
    private readonly IPatientAffiliationService _affiliationService;
    private readonly IAppointmentService _appointmentService;
    private readonly IMedicalService _medicalService;

    public InvoicesController(
        IInvoiceService invoiceService,
        IPaymentService paymentService,
        IPatientService patientService,
        IPatientAffiliationService affiliationService,
        IAppointmentService appointmentService,
        IMedicalService medicalService)
    {
        _invoiceService = invoiceService;
        _paymentService = paymentService;
        _patientService = patientService;
        _affiliationService = affiliationService;
        _appointmentService = appointmentService;
        _medicalService = medicalService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var invoices = (await _invoiceService.GetAllAsync()).ToList();

        var viewModel = new InvoiceListViewModel
        {
            Invoices = invoices.Select(i =>
            {
                var paidAmount = i.Payments.Sum(p => p.Amount);

                return new InvoiceListItemViewModel
                {
                    Id = i.Id,
                    Number = i.Number,
                    PatientName = $"{i.Patient.LastName}, {i.Patient.FirstName}",
                    HealthPlanName = i.PatientAffiliation?.HealthPlan.Name,
                    IssueDate = i.IssueDate,
                    Total = i.Total,
                    CoverageAmount = i.CoverageAmount,
                    PatientAmount = i.PatientAmount,
                    PaidAmount = paidAmount,
                    PendingAmount = Math.Max(0, i.PatientAmount - paidAmount),
                    Status = i.Status
                };
            }).ToList(),

            TotalInvoices = invoices.Count,
            PendingInvoices = invoices.Count(i =>
                i.Status == InvoiceStatus.Pending),
            PartiallyPaidInvoices = invoices.Count(i =>
                i.Status == InvoiceStatus.PartiallyPaid),
            PaidInvoices = invoices.Count(i =>
                i.Status == InvoiceStatus.Paid),
            TotalBilled = invoices
                .Where(i => i.Status != InvoiceStatus.Cancelled)
                .Sum(i => i.Total),
            TotalPending = invoices
                .Where(i => i.Status != InvoiceStatus.Cancelled)
                .Sum(i => Math.Max(
                    0,
                    i.PatientAmount - i.Payments.Sum(p => p.Amount)))
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? patientId)
    {
        var viewModel = new InvoiceFormViewModel
        {
            PatientId = patientId ?? 0,
            Details = new List<InvoiceDetailFormViewModel>
            {
                new()
            }
        };

        await LoadInvoiceFormDataAsync(viewModel, patientId);

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InvoiceFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await LoadInvoiceFormDataAsync(viewModel, viewModel.PatientId);
            return View(viewModel);
        }

        var invoice = new Invoice
        {
            PatientId = viewModel.PatientId,
            PatientAffiliationId = viewModel.PatientAffiliationId,
            IssueDate = viewModel.IssueDate,
            Notes = viewModel.Notes,
            Details = viewModel.Details.Select(d => new InvoiceDetail
            {
                MedicalServiceId = d.MedicalServiceId,
                AppointmentId = d.AppointmentId,
                ServiceDate = d.ServiceDate,
                Quantity = d.Quantity,
                AuthorizationNumber = d.AuthorizationNumber
            }).ToList()
        };

        try
        {
            var createdInvoice = await _invoiceService.CreateAsync(invoice);

            TempData["Success"] =
                $"La factura {createdInvoice.Number} fue emitida correctamente.";

            return RedirectToAction(
                nameof(Details),
                new { id = createdInvoice.Id });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            await LoadInvoiceFormDataAsync(viewModel, viewModel.PatientId);

            return View(viewModel);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var invoice = await _invoiceService.GetByIdAsync(id);

        if (invoice is null)
        {
            return NotFound();
        }

        var paidAmount = invoice.Payments.Sum(p => p.Amount);

        var viewModel = new InvoiceDetailsViewModel
        {
            Id = invoice.Id,
            Number = invoice.Number,
            PatientId = invoice.PatientId,
            PatientName =
                $"{invoice.Patient.LastName}, {invoice.Patient.FirstName}",
            PatientDocumentNumber = invoice.Patient.DocumentNumber,
            PatientPhoneNumber = invoice.Patient.PhoneNumber,
            HealthInsuranceName =
                invoice.PatientAffiliation?.HealthInsurance.Name,
            HealthPlanName =
                invoice.PatientAffiliation?.HealthPlan.Name,
            MembershipNumber =
                invoice.PatientAffiliation?.MembershipNumber,
            IssueDate = invoice.IssueDate,
            Notes = invoice.Notes,
            Subtotal = invoice.Subtotal,
            CoverageAmount = invoice.CoverageAmount,
            PatientAmount = invoice.PatientAmount,
            Total = invoice.Total,
            PaidAmount = paidAmount,
            PendingAmount = Math.Max(0, invoice.PatientAmount - paidAmount),
            Status = invoice.Status,

            Details = invoice.Details.Select(d =>
                new InvoiceDetailItemViewModel
                {
                    Description = d.Description,
                    ServiceDate = d.ServiceDate,
                    AuthorizationNumber = d.AuthorizationNumber,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    UnitCoverageAmount = d.UnitCoverageAmount,
                    Subtotal = d.Subtotal,
                    CoverageAmount = d.CoverageAmount,
                    PatientAmount = d.PatientAmount,
                    AppointmentId = d.AppointmentId
                }).ToList(),

            Payments = invoice.Payments.Select(p =>
                new InvoicePaymentItemViewModel
                {
                    Id = p.Id,
                    PaymentDate = p.PaymentDate,
                    Amount = p.Amount,
                    Method = p.Method,
                    Reference = p.Reference,
                    Notes = p.Notes
                }).ToList()
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> RegisterPayment(int invoiceId)
    {
        var invoice = await _invoiceService.GetByIdAsync(invoiceId);

        if (invoice is null)
        {
            return NotFound();
        }

        var paidAmount = invoice.Payments.Sum(p => p.Amount);
        var pendingAmount = invoice.PatientAmount - paidAmount;

        if (invoice.Status == InvoiceStatus.Cancelled ||
            pendingAmount <= 0)
        {
            TempData["Error"] =
                "La factura no admite nuevos pagos.";

            return RedirectToAction(
                nameof(Details),
                new { id = invoiceId });
        }

        var viewModel = new PaymentFormViewModel
        {
            InvoiceId = invoice.Id,
            PaymentDate = DateTime.Today,
            InvoiceNumber = invoice.Number,
            PatientName =
                $"{invoice.Patient.LastName}, {invoice.Patient.FirstName}",
            InvoiceAmount = invoice.PatientAmount,
            PaidAmount = paidAmount,
            PendingAmount = pendingAmount,
            Amount = pendingAmount
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterPayment(
        PaymentFormViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            await LoadPaymentFormDataAsync(viewModel);
            return View(viewModel);
        }

        var payment = new Payment
        {
            InvoiceId = viewModel.InvoiceId,
            PaymentDate = viewModel.PaymentDate,
            Amount = viewModel.Amount,
            Method = viewModel.Method,
            Reference = viewModel.Reference,
            Notes = viewModel.Notes
        };

        try
        {
            await _paymentService.RegisterAsync(payment);

            TempData["Success"] =
                "El pago fue registrado correctamente.";

            return RedirectToAction(
                nameof(Details),
                new { id = viewModel.InvoiceId });
        }
        catch (BusinessException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);

            await LoadPaymentFormDataAsync(viewModel);

            return View(viewModel);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await _invoiceService.CancelAsync(id);

            TempData["Success"] =
                "La factura fue anulada correctamente.";
        }
        catch (BusinessException exception)
        {
            TempData["Error"] = exception.Message;
        }

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    private async Task LoadInvoiceFormDataAsync(
        InvoiceFormViewModel viewModel,
        int? patientId)
    {
        var patients = await _patientService.GetAllAsync();

        viewModel.Patients = patients
            .Where(p => p.IsActive)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.LastName}, {p.FirstName} - {p.DocumentNumber}"
            })
            .ToList();

        var medicalServices = await _medicalService.GetActiveAsync();

        viewModel.MedicalServices = medicalServices
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = s.Name
            })
            .ToList();

        viewModel.Affiliations = new List<SelectListItem>();

        var appointments = new List<SelectListItem>();

        if (patientId.HasValue && patientId.Value > 0)
        {
            var affiliations =
                await _affiliationService.GetCurrentByPatientIdAsync(
                    patientId.Value);

            viewModel.Affiliations = affiliations.Select(a =>
                new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.MembershipNumber
                }).ToList();

            var patientAppointments =
                await _appointmentService.GetByPatientAsync(
                    patientId.Value);

            appointments = patientAppointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = $"{a.AppointmentDate:dd/MM/yyyy} - {a.StartTime:HH\\:mm}"
                })
                .ToList();
        }

        foreach (var detail in viewModel.Details)
        {
            detail.Appointments = appointments;
        }
    }

    private async Task LoadPaymentFormDataAsync(
        PaymentFormViewModel viewModel)
    {
        var invoice = await _invoiceService.GetByIdAsync(
            viewModel.InvoiceId);

        if (invoice is null)
        {
            return;
        }

        var paidAmount = invoice.Payments.Sum(p => p.Amount);

        viewModel.InvoiceNumber = invoice.Number;
        viewModel.PatientName =
            $"{invoice.Patient.LastName}, {invoice.Patient.FirstName}";
        viewModel.InvoiceAmount = invoice.PatientAmount;
        viewModel.PaidAmount = paidAmount;
        viewModel.PendingAmount = Math.Max(
            0,
            invoice.PatientAmount - paidAmount);
    }
}
