
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services;

public class HealthPlanService : IHealthPlanService
{
    private readonly ApplicationDbContext _context;

    public HealthPlanService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HealthPlan>> GetAllAsync()
    {
        return await _context.HealthPlans
            .AsNoTracking()
            .Include(p => p.HealthInsurance)
            .OrderBy(p => p.HealthInsurance.Name)
            .ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<HealthPlan>> GetActiveAsync()
    {
        return await _context.HealthPlans
            .AsNoTracking()
            .Include(p => p.HealthInsurance)
            .Where(p => p.IsActive && p.HealthInsurance.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<HealthPlan?> GetByIdAsync(int id)
    {
        return await _context.HealthPlans
            .AsNoTracking()
            .Include(p => p.HealthInsurance)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<HealthPlan?> GetByNameAsync(string name)
    {
        string normalizedName = name.Trim();

        return await _context.HealthPlans
            .AsNoTracking()
            .Include(p => p.HealthInsurance)
            .FirstOrDefaultAsync(p => p.Name == normalizedName);
    }

    public async Task<HealthPlan> CreateAsync(HealthPlan healthPlan)
    {
        string name = healthPlan.Name.Trim();
        string? code = Normalize(healthPlan.Code);

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("El nombre del plan es obligatorio.");

        var healthInsurance = await _context.HealthInsurances
            .FirstOrDefaultAsync(h => h.Id == healthPlan.HealthInsuranceId);

        if (healthInsurance is null)
            throw new BusinessException("La cobertura médica seleccionada no existe.");

        if (!healthInsurance.IsActive)
            throw new BusinessException("No se pueden crear planes para una cobertura desactivada.");

        if (code is not null)
        {
            bool duplicateCode = await _context.HealthPlans
                .AnyAsync(p =>
                    p.HealthInsuranceId == healthPlan.HealthInsuranceId &&
                    p.Code == code);

            if (duplicateCode)
                throw new BusinessException(
                    "Ya existe un plan con ese código para la cobertura seleccionada.");
        }

        healthPlan.Name = name;
        healthPlan.Code = code;
        healthPlan.Description = Normalize(healthPlan.Description);
        healthPlan.IsActive = true;

        _context.HealthPlans.Add(healthPlan);
        await _context.SaveChangesAsync();

        return healthPlan;
    }

    public async Task UpdateAsync(int id, HealthPlan healthPlan)
    {
        var existingHealthPlan = await _context.HealthPlans
            .FirstOrDefaultAsync(p => p.Id == id);

        if (existingHealthPlan is null)
            throw new BusinessException("El plan médico no existe.");

        string name = healthPlan.Name.Trim();
        string? code = Normalize(healthPlan.Code);

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("El nombre del plan es obligatorio.");

        var healthInsurance = await _context.HealthInsurances
            .FirstOrDefaultAsync(h => h.Id == healthPlan.HealthInsuranceId);

        if (healthInsurance is null)
            throw new BusinessException("La cobertura médica seleccionada no existe.");

        if (!healthInsurance.IsActive)
            throw new BusinessException("No se pueden asociar planes a una cobertura desactivada.");

        if (code is not null)
        {
            bool duplicateCode = await _context.HealthPlans
                .AnyAsync(p =>
                    p.Id != id &&
                    p.HealthInsuranceId == healthPlan.HealthInsuranceId &&
                    p.Code == code);

            if (duplicateCode)
                throw new BusinessException(
                    "Ya existe otro plan con ese código para la cobertura seleccionada.");
        }

        existingHealthPlan.HealthInsuranceId = healthPlan.HealthInsuranceId;
        existingHealthPlan.Name = name;
        existingHealthPlan.Code = code;
        existingHealthPlan.Description = Normalize(healthPlan.Description);
        existingHealthPlan.RequiresAuthorization = healthPlan.RequiresAuthorization;

        await _context.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var healthPlan = await _context.HealthPlans
            .FirstOrDefaultAsync(p => p.Id == id);

        if (healthPlan is null)
            throw new BusinessException("El plan médico no existe.");

        if (!healthPlan.IsActive)
            throw new BusinessException("El plan médico ya está desactivado.");

        healthPlan.IsActive = false;

        await _context.SaveChangesAsync();
    }

    public async Task ActivateAsync(int id)
    {
        var healthPlan = await _context.HealthPlans
            .Include(p => p.HealthInsurance)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (healthPlan is null)
            throw new BusinessException("El plan médico no existe.");

        if (healthPlan.IsActive)
            throw new BusinessException("El plan médico ya está activo.");

        if (!healthPlan.HealthInsurance.IsActive)
            throw new BusinessException(
                "No se puede activar el plan porque su cobertura médica está desactivada.");

        healthPlan.IsActive = true;

        await _context.SaveChangesAsync();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
