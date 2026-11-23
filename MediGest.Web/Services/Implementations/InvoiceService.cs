
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Enums;
using MediGest.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services.Implementations;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;

    public InvoiceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        return await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Patient)
            .Include(i => i.PatientAffiliation)
                .ThenInclude(a => a!.HealthPlan)
            .Include(i => i.PatientAffiliation)
                .ThenInclude(a => a!.HealthInsurance)
            .Include(i => i.Payments)
            .OrderByDescending(i => i.IssueDate)
            .ThenByDescending(i => i.Id)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetByPatientAsync(int patientId)
    {
        return await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Patient)
            .Include(i => i.Details)
                .ThenInclude(d => d.MedicalService)
            .Where(i => i.PatientId == patientId)
            .OrderByDescending(i => i.IssueDate)
            .ThenByDescending(i => i.Id)
            .ToListAsync();
    }


    public async Task<Invoice?> GetByIdAsync(int id)
    {
        return await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Patient)
            .Include(i => i.PatientAffiliation)
                .ThenInclude(a => a!.HealthPlan)
            .Include(i => i.PatientAffiliation)
                .ThenInclude(a => a!.HealthInsurance)
            .Include(i => i.Details)
                .ThenInclude(d => d.MedicalService)
            .Include(i => i.Details)
                .ThenInclude(d => d.Appointment)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id);
    }


    public async Task<Invoice> CreateAsync(Invoice invoice)
    {
        if (invoice is null)
        {
            throw new BusinessException("Los datos de la factura son obligatorios.");
        }

        if (invoice.Details is null || !invoice.Details.Any())
        {
            throw new BusinessException(
                "La factura debe contener al menos una prestación.");
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        if (invoice.IssueDate == default)
        {
            throw new BusinessException(
                "La fecha de emisión de la factura es obligatoria.");
        }

        if (invoice.IssueDate > today)
        {
            throw new BusinessException(
                "La fecha de emisión no puede ser futura.");
        }

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == invoice.PatientId);

        if (patient is null)
        {
            throw new BusinessException("El paciente no existe.");
        }

        if (!patient.IsActive)
        {
            throw new BusinessException(
                "No se pueden emitir facturas a pacientes inactivos.");
        }

        PatientAffiliation? affiliation = null;

        if (invoice.PatientAffiliationId.HasValue)
        {
            affiliation = await _context.PatientAffiliations
                .Include(a => a.HealthPlan)
                .FirstOrDefaultAsync(a =>
                    a.Id == invoice.PatientAffiliationId.Value);

            if (affiliation is null ||
                affiliation.PatientId != invoice.PatientId)
            {
                throw new BusinessException(
                    "La afiliación seleccionada no pertenece al paciente.");
            }

            if (affiliation.Status != PatientAffiliationStatus.Active)
            {
                throw new BusinessException(
                    "La afiliación seleccionada no se encuentra activa.");
            }

            if (affiliation.ValidFrom > invoice.IssueDate ||
                (affiliation.ValidTo.HasValue &&
                 affiliation.ValidTo.Value < invoice.IssueDate))
            {
                throw new BusinessException(
                    "La afiliación no se encontraba vigente en la fecha de emisión.");
            }

            if (!affiliation.HealthPlan.IsActive)
            {
                throw new BusinessException(
                    "El plan de salud seleccionado se encuentra inactivo.");
            }
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var newInvoice = new Invoice
        {
            Number = $"TMP-{Guid.NewGuid():N}"[..30],
            PatientId = invoice.PatientId,
            PatientAffiliationId = invoice.PatientAffiliationId,
            IssueDate = invoice.IssueDate,
            Notes = string.IsNullOrWhiteSpace(invoice.Notes)
                ? null
                : invoice.Notes.Trim(),
            Status = InvoiceStatus.Pending,
            Details = new List<InvoiceDetail>()
        };

        decimal subtotal = 0;
        decimal coverageAmount = 0;

        var processedAppointments = new HashSet<(int AppointmentId, int MedicalServiceId)>();

        foreach (var requestedDetail in invoice.Details)
        {
            if (requestedDetail.Quantity <= 0)
            {
                throw new BusinessException(
                    "La cantidad de cada prestación debe ser mayor a cero.");
            }

            if (requestedDetail.ServiceDate == default)
            {
                throw new BusinessException(
                    "La fecha de prestación es obligatoria.");
            }

            if (requestedDetail.ServiceDate > invoice.IssueDate)
            {
                throw new BusinessException(
                    "La fecha de prestación no puede ser posterior a la emisión de la factura.");
            }

            var medicalService = await _context.MedicalServices
                .FirstOrDefaultAsync(s =>
                    s.Id == requestedDetail.MedicalServiceId);

            if (medicalService is null || !medicalService.IsActive)
            {
                throw new BusinessException(
                    "Una de las prestaciones seleccionadas no existe o está inactiva.");
            }

            Appointment? appointment = null;

            if (requestedDetail.AppointmentId.HasValue)
            {
                var appointmentId = requestedDetail.AppointmentId.Value;

                appointment = await _context.Appointments
                    .FirstOrDefaultAsync(a => a.Id == appointmentId);

                if (appointment is null)
                {
                    throw new BusinessException(
                        "El turno asociado a la prestación no existe.");
                }

                if (appointment.PatientId != invoice.PatientId)
                {
                    throw new BusinessException(
                        "El turno seleccionado no pertenece al paciente.");
                }

                if (appointment.Status != AppointmentStatus.Completed)
                {
                    throw new BusinessException(
                        "Solo se pueden facturar turnos completados.");
                }

                if (appointment.AppointmentDate != requestedDetail.ServiceDate)
                {
                    throw new BusinessException(
                        "La fecha de prestación debe coincidir con la fecha del turno.");
                }

                var appointmentKey = (
                    appointmentId,
                    requestedDetail.MedicalServiceId);

                if (!processedAppointments.Add(appointmentKey))
                {
                    throw new BusinessException(
                        "No se puede repetir la misma prestación para un turno dentro de una factura.");
                }

                var alreadyBilled = await _context.InvoiceDetails
                    .AnyAsync(d =>
                        d.AppointmentId == appointmentId &&
                        d.MedicalServiceId == requestedDetail.MedicalServiceId &&
                        d.Invoice.Status != InvoiceStatus.Cancelled);

                if (alreadyBilled)
                {
                    throw new BusinessException(
                        "La prestación de este turno ya fue facturada.");
                }
            }

            var unitPrice = medicalService.PrivatePrice;
            decimal unitCoverage = 0;

            if (affiliation is not null)
            {
                if (affiliation.ValidFrom > requestedDetail.ServiceDate ||
                    (affiliation.ValidTo.HasValue &&
                     affiliation.ValidTo.Value < requestedDetail.ServiceDate))
                {
                    throw new BusinessException(
                        "La afiliación no estaba vigente en la fecha de prestación.");
                }

                var tariff = await _context.HealthPlanTariffs
                    .Where(t =>
                        t.HealthPlanId == affiliation.HealthPlanId &&
                        t.MedicalServiceId == medicalService.Id &&
                        t.ValidFrom <= requestedDetail.ServiceDate &&
                        (!t.ValidTo.HasValue ||
                         t.ValidTo.Value >= requestedDetail.ServiceDate))
                    .OrderByDescending(t => t.ValidFrom)
                    .FirstOrDefaultAsync();

                if (tariff is null)
                {
                    throw new BusinessException(
                        $"No existe una tarifa vigente para la prestación '{medicalService.Name}' en el plan seleccionado.");
                }

                var requiresAuthorization =
                    affiliation.HealthPlan.RequiresAuthorization ||
                    tariff.RequiresAuthorization;

                if (requiresAuthorization &&
                    string.IsNullOrWhiteSpace(requestedDetail.AuthorizationNumber))
                {
                    throw new BusinessException(
                        $"La prestación '{medicalService.Name}' requiere un número de autorización.");
                }

                unitCoverage = Math.Min(tariff.Amount, unitPrice);
            }

            var detailSubtotal = unitPrice * requestedDetail.Quantity;
            var detailCoverage = unitCoverage * requestedDetail.Quantity;
            var detailPatientAmount = detailSubtotal - detailCoverage;

            subtotal += detailSubtotal;
            coverageAmount += detailCoverage;

            newInvoice.Details.Add(new InvoiceDetail
            {
                MedicalServiceId = medicalService.Id,
                AppointmentId = requestedDetail.AppointmentId,
                Description = medicalService.Name,
                ServiceDate = requestedDetail.ServiceDate,
                AuthorizationNumber =
                    string.IsNullOrWhiteSpace(requestedDetail.AuthorizationNumber)
                        ? null
                        : requestedDetail.AuthorizationNumber.Trim(),
                Quantity = requestedDetail.Quantity,
                UnitPrice = unitPrice,
                UnitCoverageAmount = unitCoverage,
                Subtotal = detailSubtotal,
                CoverageAmount = detailCoverage,
                PatientAmount = detailPatientAmount
            });
        }

        newInvoice.Subtotal = Math.Round(subtotal, 2);
        newInvoice.CoverageAmount = Math.Round(coverageAmount, 2);
        newInvoice.PatientAmount = Math.Round(
            subtotal - coverageAmount, 2);
        newInvoice.Total = newInvoice.Subtotal;

        _context.Invoices.Add(newInvoice);

        await _context.SaveChangesAsync();

        newInvoice.Number =
            $"FC-{newInvoice.IssueDate:yyyyMMdd}-{newInvoice.Id:D6}";

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return newInvoice;
    }

    public async Task CancelAsync(int id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null)
        {
            throw new BusinessException("La factura no existe.");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            throw new BusinessException(
                "La factura ya se encuentra anulada.");
        }

        if (invoice.Payments.Any())
        {
            throw new BusinessException(
                "No se puede anular una factura que tiene pagos registrados.");
        }

        invoice.Status = InvoiceStatus.Cancelled;

        await _context.SaveChangesAsync();
    }
}
