
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services;

public class HealthPlanTariffService : IHealthPlanTariffService
{
    private readonly ApplicationDbContext _context;

    public HealthPlanTariffService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HealthPlanTariff>> GetAllAsync()
    {
        return await _context.HealthPlanTariffs
            .AsNoTracking()
            .Include(t => t.HealthPlan)
                .ThenInclude(p => p.HealthInsurance)
            .Include(t => t.MedicalService)
                .ThenInclude(s => s.Specialty)
            .OrderBy(t => t.HealthPlan.HealthInsurance.Name)
            .ThenBy(t => t.HealthPlan.Name)
            .ThenBy(t => t.MedicalService.Name)
            .ThenByDescending(t => t.ValidFrom)
            .ToListAsync();
    }

    public async Task<IEnumerable<HealthPlanTariff>> GetByHealthPlanIdAsync(
        int healthPlanId)
    {
        return await _context.HealthPlanTariffs
            .AsNoTracking()
            .Include(t => t.HealthPlan)
            .Include(t => t.MedicalService)
            .Where(t => t.HealthPlanId == healthPlanId)
            .OrderBy(t => t.MedicalService.Name)
            .ThenByDescending(t => t.ValidFrom)
            .ToListAsync();
    }

    public async Task<IEnumerable<HealthPlanTariff>> GetByMedicalServiceIdAsync(
        int medicalServiceId)
    {
        return await _context.HealthPlanTariffs
            .AsNoTracking()
            .Include(t => t.HealthPlan)
                .ThenInclude(p => p.HealthInsurance)
            .Include(t => t.MedicalService)
            .Where(t => t.MedicalServiceId == medicalServiceId)
            .OrderBy(t => t.HealthPlan.HealthInsurance.Name)
            .ThenBy(t => t.HealthPlan.Name)
            .ThenByDescending(t => t.ValidFrom)
            .ToListAsync();
    }

    public async Task<HealthPlanTariff?> GetByIdAsync(int id)
    {
        return await _context.HealthPlanTariffs
            .AsNoTracking()
            .Include(t => t.HealthPlan)
                .ThenInclude(p => p.HealthInsurance)
            .Include(t => t.MedicalService)
                .ThenInclude(s => s.Specialty)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<HealthPlanTariff?> GetCurrentAsync(
        int healthPlanId,
        int medicalServiceId,
        DateOnly date)
    {
        return await _context.HealthPlanTariffs
            .AsNoTracking()
            .Include(t => t.HealthPlan)
                .ThenInclude(p => p.HealthInsurance)
            .Include(t => t.MedicalService)
            .Where(t =>
                t.HealthPlanId == healthPlanId &&
                t.MedicalServiceId == medicalServiceId &&
                t.ValidFrom <= date &&
                (!t.ValidTo.HasValue || t.ValidTo.Value >= date))
            .OrderByDescending(t => t.ValidFrom)
            .FirstOrDefaultAsync();
    }

    public async Task<HealthPlanTariff> CreateAsync(HealthPlanTariff tariff)
    {
        ValidateTariff(tariff);

        await ValidateReferencesAsync(
            tariff.HealthPlanId,
            tariff.MedicalServiceId);

        await EnsureNoOverlappingTariffAsync(
            tariff.HealthPlanId,
            tariff.MedicalServiceId,
            tariff.ValidFrom,
            tariff.ValidTo);

        tariff.Notes = Normalize(tariff.Notes);

        _context.HealthPlanTariffs.Add(tariff);

        await _context.SaveChangesAsync();

        return tariff;
    }


    public async Task UpdateAsync(int id, HealthPlanTariff tariff)
    {
        var existingTariff = await _context.HealthPlanTariffs
            .FirstOrDefaultAsync(t => t.Id == id);

        if (existingTariff is null)
            throw new BusinessException("La tarifa seleccionada no existe.");

        if (existingTariff.ValidFrom <= DateOnly.FromDateTime(DateTime.Today))
        {
            throw new BusinessException(
                "No se pueden modificar tarifas que ya comenzaron su vigencia. " +
                "Cerrá la tarifa y registrá una nueva para conservar el historial.");
        }

        ValidateTariff(tariff);

        await ValidateReferencesAsync(
            tariff.HealthPlanId,
            tariff.MedicalServiceId);

        await EnsureNoOverlappingTariffAsync(
            tariff.HealthPlanId,
            tariff.MedicalServiceId,
            tariff.ValidFrom,
            tariff.ValidTo,
            id);

        existingTariff.HealthPlanId = tariff.HealthPlanId;
        existingTariff.MedicalServiceId = tariff.MedicalServiceId;
        existingTariff.Amount = tariff.Amount;
        existingTariff.ValidFrom = tariff.ValidFrom;
        existingTariff.ValidTo = tariff.ValidTo;
        existingTariff.RequiresAuthorization = tariff.RequiresAuthorization;
        existingTariff.Notes = Normalize(tariff.Notes);

        await _context.SaveChangesAsync();
    }


    public async Task CloseAsync(int id, DateOnly validTo)
    {
        var tariff = await _context.HealthPlanTariffs
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tariff is null)
            throw new BusinessException("La tarifa seleccionada no existe.");

        if (tariff.ValidTo.HasValue)
            throw new BusinessException("La tarifa ya tiene una fecha de finalización.");

        if (validTo < tariff.ValidFrom)
            throw new BusinessException(
                "La fecha de finalización no puede ser anterior al inicio de vigencia.");

        tariff.ValidTo = validTo;

        await _context.SaveChangesAsync();
    }

    private static void ValidateTariff(HealthPlanTariff tariff)
    {
        if (tariff.HealthPlanId <= 0)
            throw new BusinessException("Seleccioná un plan de cobertura válido.");

        if (tariff.MedicalServiceId <= 0)
            throw new BusinessException("Seleccioná una prestación médica válida.");

        if (tariff.Amount <= 0)
            throw new BusinessException("El importe de la tarifa debe ser mayor a cero.");

        if (tariff.ValidTo.HasValue &&
            tariff.ValidTo.Value < tariff.ValidFrom)
        {
            throw new BusinessException(
                "La fecha de finalización no puede ser anterior al inicio de vigencia.");
        }
    }

    private async Task ValidateReferencesAsync(
        int healthPlanId,
        int medicalServiceId)
    {
        var healthPlan = await _context.HealthPlans
            .Include(p => p.HealthInsurance)
            .FirstOrDefaultAsync(p => p.Id == healthPlanId);

        if (healthPlan is null)
            throw new BusinessException("El plan de cobertura seleccionado no existe.");

        if (!healthPlan.IsActive || !healthPlan.HealthInsurance.IsActive)
        {
            throw new BusinessException(
                "No se pueden registrar tarifas para un plan o cobertura desactivados.");
        }

        var medicalService = await _context.MedicalServices
            .FirstOrDefaultAsync(s => s.Id == medicalServiceId);

        if (medicalService is null)
            throw new BusinessException("La prestación médica seleccionada no existe.");

        if (!medicalService.IsActive)
            throw new BusinessException(
                "No se pueden registrar tarifas para una prestación desactivada.");
    }

    private async Task EnsureNoOverlappingTariffAsync(
        int healthPlanId,
        int medicalServiceId,
        DateOnly validFrom,
        DateOnly? validTo,
        int? excludedTariffId = null)
    {
        DateOnly newEnd = validTo ?? DateOnly.MaxValue;

        bool hasOverlap = await _context.HealthPlanTariffs
            .AnyAsync(t =>
                t.HealthPlanId == healthPlanId &&
                t.MedicalServiceId == medicalServiceId &&
                (!excludedTariffId.HasValue || t.Id != excludedTariffId.Value) &&
                t.ValidFrom <= newEnd &&
                (!t.ValidTo.HasValue || t.ValidTo.Value >= validFrom));

        if (hasOverlap)
        {
            throw new BusinessException(
                "Ya existe una tarifa para este plan y prestación durante el período seleccionado.");
        }
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
