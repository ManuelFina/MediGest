
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Enums;
using MediGest.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services;

public class PatientAffiliationService : IPatientAffiliationService
{
    private readonly ApplicationDbContext _context;

    public PatientAffiliationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PatientAffiliation>> GetAllAsync()
    {
        return await _context.PatientAffiliations
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.HealthInsurance)
            .Include(a => a.HealthPlan)
            .OrderBy(a => a.Patient.LastName)
            .ThenBy(a => a.Patient.FirstName)
            .ThenByDescending(a => a.ValidFrom)
            .ToListAsync();
    }

    public async Task<IEnumerable<PatientAffiliation>> GetByPatientIdAsync(
        int patientId)
    {
        return await _context.PatientAffiliations
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.HealthInsurance)
            .Include(a => a.HealthPlan)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.IsPrimary)
            .ThenByDescending(a => a.ValidFrom)
            .ToListAsync();
    }

    public async Task<IEnumerable<PatientAffiliation>> GetCurrentByPatientIdAsync(
        int patientId)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        return await _context.PatientAffiliations
            .AsNoTracking()
            .Include(a => a.HealthInsurance)
            .Include(a => a.HealthPlan)
            .Where(a =>
                a.PatientId == patientId &&
                a.Status == PatientAffiliationStatus.Active &&
                a.ValidFrom <= today &&
                (!a.ValidTo.HasValue || a.ValidTo.Value >= today))
            .OrderByDescending(a => a.IsPrimary)
            .ThenByDescending(a => a.ValidFrom)
            .ToListAsync();
    }

    public async Task<PatientAffiliation?> GetByIdAsync(int id)
    {
        return await _context.PatientAffiliations
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.HealthInsurance)
            .Include(a => a.HealthPlan)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<PatientAffiliation> CreateAsync(
        PatientAffiliation affiliation)
    {
        string membershipNumber = Normalize(affiliation.MembershipNumber)
            ?? throw new BusinessException(
                "El número de afiliado es obligatorio.");

        await ValidateReferencesAsync(
            affiliation.PatientId,
            affiliation.HealthInsuranceId,
            affiliation.HealthPlanId);

        ValidateDates(affiliation.ValidFrom, affiliation.ValidTo);

        if (affiliation.IsPrimary)
        {
            await ValidatePrimaryAffiliationAsync(
                affiliation.PatientId,
                affiliation.ValidFrom,
                affiliation.ValidTo);
        }

        affiliation.MembershipNumber = membershipNumber;
        affiliation.Notes = Normalize(affiliation.Notes);
        affiliation.Status = PatientAffiliationStatus.Active;

        _context.PatientAffiliations.Add(affiliation);

        await _context.SaveChangesAsync();

        return affiliation;
    }

    public async Task UpdateAsync(
        int id,
        PatientAffiliation affiliation)
    {
        var existingAffiliation = await _context.PatientAffiliations
            .FirstOrDefaultAsync(a => a.Id == id);

        if (existingAffiliation is null)
            throw new BusinessException("La afiliación no existe.");

        if (existingAffiliation.Status == PatientAffiliationStatus.Cancelled)
            throw new BusinessException(
                "No se puede modificar una afiliación cancelada.");

        string membershipNumber = Normalize(affiliation.MembershipNumber)
            ?? throw new BusinessException(
                "El número de afiliado es obligatorio.");

        await ValidateReferencesAsync(
            affiliation.PatientId,
            affiliation.HealthInsuranceId,
            affiliation.HealthPlanId);

        ValidateDates(affiliation.ValidFrom, affiliation.ValidTo);

        if (affiliation.IsPrimary)
        {
            await ValidatePrimaryAffiliationAsync(
                affiliation.PatientId,
                affiliation.ValidFrom,
                affiliation.ValidTo,
                id);
        }

        existingAffiliation.PatientId = affiliation.PatientId;
        existingAffiliation.HealthInsuranceId = affiliation.HealthInsuranceId;
        existingAffiliation.HealthPlanId = affiliation.HealthPlanId;
        existingAffiliation.MembershipNumber = membershipNumber;
        existingAffiliation.MemberType = affiliation.MemberType;
        existingAffiliation.ValidFrom = affiliation.ValidFrom;
        existingAffiliation.ValidTo = affiliation.ValidTo;
        existingAffiliation.IsPrimary = affiliation.IsPrimary;
        existingAffiliation.Notes = Normalize(affiliation.Notes);

        await _context.SaveChangesAsync();
    }

    public async Task SuspendAsync(int id)
    {
        var affiliation = await GetTrackedAffiliationAsync(id);

        if (affiliation.Status != PatientAffiliationStatus.Active)
            throw new BusinessException(
                "Solamente se pueden suspender afiliaciones activas.");

        affiliation.Status = PatientAffiliationStatus.Suspended;

        await _context.SaveChangesAsync();
    }

    public async Task ReactivateAsync(int id)
    {
        var affiliation = await GetTrackedAffiliationAsync(id);

        if (affiliation.Status != PatientAffiliationStatus.Suspended)
            throw new BusinessException(
                "Solamente se pueden reactivar afiliaciones suspendidas.");

        ValidateDates(affiliation.ValidFrom, affiliation.ValidTo);

        await ValidateReferencesAsync(
            affiliation.PatientId,
            affiliation.HealthInsuranceId,
            affiliation.HealthPlanId);

        if (affiliation.IsPrimary)
        {
            await ValidatePrimaryAffiliationAsync(
                affiliation.PatientId,
                affiliation.ValidFrom,
                affiliation.ValidTo,
                id);
        }

        affiliation.Status = PatientAffiliationStatus.Active;

        await _context.SaveChangesAsync();
    }

    public async Task CancelAsync(int id)
    {
        var affiliation = await GetTrackedAffiliationAsync(id);

        if (affiliation.Status == PatientAffiliationStatus.Cancelled)
            throw new BusinessException(
                "La afiliación ya está cancelada.");

        affiliation.Status = PatientAffiliationStatus.Cancelled;

        await _context.SaveChangesAsync();
    }

    private async Task<PatientAffiliation> GetTrackedAffiliationAsync(int id)
    {
        var affiliation = await _context.PatientAffiliations
            .FirstOrDefaultAsync(a => a.Id == id);

        if (affiliation is null)
            throw new BusinessException("La afiliación no existe.");

        return affiliation;
    }

    private async Task ValidateReferencesAsync(
        int patientId,
        int healthInsuranceId,
        int healthPlanId)
    {
        bool patientExists = await _context.Patients
            .AnyAsync(p => p.Id == patientId && p.IsActive);

        if (!patientExists)
            throw new BusinessException(
                "El paciente seleccionado no existe o está desactivado.");

        var healthInsurance = await _context.HealthInsurances
            .FirstOrDefaultAsync(h => h.Id == healthInsuranceId);

        if (healthInsurance is null)
            throw new BusinessException(
                "La cobertura médica seleccionada no existe.");

        if (!healthInsurance.IsActive)
            throw new BusinessException(
                "La cobertura médica seleccionada está desactivada.");

        bool validPlan = await _context.HealthPlans
            .AnyAsync(p =>
                p.Id == healthPlanId &&
                p.HealthInsuranceId == healthInsuranceId &&
                p.IsActive);

        if (!validPlan)
            throw new BusinessException(
                "El plan seleccionado no existe, está desactivado o no pertenece a la cobertura elegida.");
    }

    private async Task ValidatePrimaryAffiliationAsync(
        int patientId,
        DateOnly validFrom,
        DateOnly? validTo,
        int? excludedAffiliationId = null)
    {
        DateOnly newEnd = validTo ?? DateOnly.MaxValue;

        bool hasOverlappingPrimary = await _context.PatientAffiliations
            .AnyAsync(a =>
                a.PatientId == patientId &&
                a.IsPrimary &&
                a.Status == PatientAffiliationStatus.Active &&
                (!excludedAffiliationId.HasValue ||
                 a.Id != excludedAffiliationId.Value) &&
                a.ValidFrom <= newEnd &&
                (!a.ValidTo.HasValue || a.ValidTo.Value >= validFrom));

        if (hasOverlappingPrimary)
            throw new BusinessException(
                "El paciente ya tiene otra afiliación principal vigente durante ese período.");
    }

    private static void ValidateDates(
        DateOnly validFrom,
        DateOnly? validTo)
    {
        if (validTo.HasValue && validTo.Value < validFrom)
            throw new BusinessException(
                "La fecha de finalización no puede ser anterior a la fecha de inicio.");
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
