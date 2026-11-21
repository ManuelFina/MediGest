
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services;

public class HealthInsuranceService : IHealthInsuranceService
{
    private readonly ApplicationDbContext _context;

    public HealthInsuranceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<HealthInsurance>> GetAllAsync()
    {
        return await _context.HealthInsurances
            .AsNoTracking()
            .Include(h => h.Plans)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<HealthInsurance>> GetActiveAsync()
    {
        return await _context.HealthInsurances
            .AsNoTracking()
            .Where(h => h.IsActive)
            .OrderBy(h => h.Name)
            .ToListAsync();
    }

    public async Task<HealthInsurance?> GetByIdAsync(int id)
    {
        return await _context.HealthInsurances
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<HealthInsurance?> GetByNameAsync(string name)
    {
        string normalizedName = name.Trim();

        return await _context.HealthInsurances
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Name == normalizedName);
    }

    public async Task<HealthInsurance> CreateAsync(HealthInsurance healthInsurance)
    {
        string name = healthInsurance.Name.Trim();
        string legalName = healthInsurance.LegalName.Trim();
        string cuit = healthInsurance.CUIT.Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("El nombre de la cobertura es obligatorio.");

        if (string.IsNullOrWhiteSpace(legalName))
            throw new BusinessException("La razón social es obligatoria.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(cuit, @"^\d{11}$"))
            throw new BusinessException("El CUIT debe contener exactamente 11 dígitos.");

        if (await _context.HealthInsurances.AnyAsync(h => h.CUIT == cuit))
            throw new BusinessException("Ya existe una cobertura registrada con ese CUIT.");

        healthInsurance.Name = name;
        healthInsurance.LegalName = legalName;
        healthInsurance.CUIT = cuit;
        healthInsurance.RegistrationNumber = Normalize(healthInsurance.RegistrationNumber);
        healthInsurance.PhoneNumber = Normalize(healthInsurance.PhoneNumber);
        healthInsurance.Email = Normalize(healthInsurance.Email);
        healthInsurance.Address = Normalize(healthInsurance.Address);
        healthInsurance.Notes = Normalize(healthInsurance.Notes);
        healthInsurance.IsActive = true;

        _context.HealthInsurances.Add(healthInsurance);
        await _context.SaveChangesAsync();

        return healthInsurance;
    }

    public async Task UpdateAsync(int id, HealthInsurance healthInsurance)
    {
        var existingHealthInsurance = await _context.HealthInsurances
            .FirstOrDefaultAsync(h => h.Id == id);

        if (existingHealthInsurance is null)
            throw new BusinessException("La cobertura médica no existe.");

        string name = healthInsurance.Name.Trim();
        string legalName = healthInsurance.LegalName.Trim();
        string cuit = healthInsurance.CUIT.Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("El nombre de la cobertura es obligatorio.");

        if (string.IsNullOrWhiteSpace(legalName))
            throw new BusinessException("La razón social es obligatoria.");

        if (!System.Text.RegularExpressions.Regex.IsMatch(cuit, @"^\d{11}$"))
            throw new BusinessException("El CUIT debe contener exactamente 11 dígitos.");

        bool duplicateCuit = await _context.HealthInsurances
            .AnyAsync(h => h.Id != id && h.CUIT == cuit);

        if (duplicateCuit)
            throw new BusinessException("Ya existe otra cobertura registrada con ese CUIT.");

        existingHealthInsurance.Name = name;
        existingHealthInsurance.LegalName = legalName;
        existingHealthInsurance.CUIT = cuit;
        existingHealthInsurance.EntityType = healthInsurance.EntityType;
        existingHealthInsurance.RegistrationNumber = Normalize(healthInsurance.RegistrationNumber);
        existingHealthInsurance.PhoneNumber = Normalize(healthInsurance.PhoneNumber);
        existingHealthInsurance.Email = Normalize(healthInsurance.Email);
        existingHealthInsurance.Address = Normalize(healthInsurance.Address);
        existingHealthInsurance.Notes = Normalize(healthInsurance.Notes);

        await _context.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var healthInsurance = await _context.HealthInsurances
            .Include(h => h.Plans)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (healthInsurance is null)
            throw new BusinessException("La cobertura médica no existe.");

        if (!healthInsurance.IsActive)
            throw new BusinessException("La cobertura médica ya está desactivada.");

        if (healthInsurance.Plans.Any(p => p.IsActive))
            throw new BusinessException(
                "No se puede desactivar esta cobertura porque tiene planes activos asociados. Desactivá primero sus planes.");

        healthInsurance.IsActive = false;

        await _context.SaveChangesAsync();
    }

    public async Task ActivateAsync(int id)
    {
        var healthInsurance = await _context.HealthInsurances
            .FirstOrDefaultAsync(h => h.Id == id);

        if (healthInsurance is null)
            throw new BusinessException("La cobertura médica no existe.");

        if (healthInsurance.IsActive)
            throw new BusinessException("La cobertura médica ya está activa.");

        healthInsurance.IsActive = true;

        await _context.SaveChangesAsync();
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
