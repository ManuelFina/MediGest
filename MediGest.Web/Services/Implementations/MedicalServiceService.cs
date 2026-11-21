
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services.Implementations;

public class MedicalServiceService : IMedicalService
{
    private readonly ApplicationDbContext _context;

    public MedicalServiceService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MedicalService>> GetAllAsync()
    {
        return await _context.MedicalServices
            .AsNoTracking()
            .Include(s => s.Specialty)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<MedicalService>> GetActiveAsync()
    {
        return await _context.MedicalServices
            .AsNoTracking()
            .Include(s => s.Specialty)
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<MedicalService?> GetByIdAsync(int id)
    {
        return await _context.MedicalServices
            .AsNoTracking()
            .Include(s => s.Specialty)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<MedicalService?> GetByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        string normalizedName = name.Trim();

        return await _context.MedicalServices
            .AsNoTracking()
            .Include(s => s.Specialty)
            .FirstOrDefaultAsync(s => s.Name == normalizedName);
    }

    public async Task<MedicalService> CreateAsync(MedicalService medicalService)
    {
        NormalizeMedicalServiceData(medicalService);

        ValidateMedicalService(medicalService);

        await ValidateSpecialtyAsync(medicalService.SpecialtyId);

        if (medicalService.Code is not null)
        {
            bool codeExists = await _context.MedicalServices
                .AnyAsync(s => s.Code == medicalService.Code);

            if (codeExists)
            {
                throw new BusinessException(
                    "Ya existe una prestación registrada con ese código.");
            }
        }

        bool nameExists = await _context.MedicalServices
            .AnyAsync(s => s.Name == medicalService.Name);

        if (nameExists)
        {
            throw new BusinessException(
                "Ya existe una prestación registrada con ese nombre.");
        }

        medicalService.IsActive = true;

        _context.MedicalServices.Add(medicalService);

        await _context.SaveChangesAsync();

        return medicalService;
    }

    public async Task UpdateAsync(int id, MedicalService medicalService)
    {
        NormalizeMedicalServiceData(medicalService);

        var existingMedicalService = await _context.MedicalServices
            .FirstOrDefaultAsync(s => s.Id == id);

        if (existingMedicalService is null)
        {
            throw new BusinessException("La prestación médica no existe.");
        }

        ValidateMedicalService(medicalService);

        await ValidateSpecialtyAsync(medicalService.SpecialtyId);

        if (medicalService.Code is not null)
        {
            bool codeExists = await _context.MedicalServices
                .AnyAsync(s =>
                    s.Code == medicalService.Code &&
                    s.Id != id);

            if (codeExists)
            {
                throw new BusinessException(
                    "Ya existe otra prestación registrada con ese código.");
            }
        }

        bool nameExists = await _context.MedicalServices
            .AnyAsync(s =>
                s.Name == medicalService.Name &&
                s.Id != id);

        if (nameExists)
        {
            throw new BusinessException(
                "Ya existe otra prestación registrada con ese nombre.");
        }

        existingMedicalService.Code = medicalService.Code;
        existingMedicalService.Name = medicalService.Name;
        existingMedicalService.Description = medicalService.Description;
        existingMedicalService.SpecialtyId = medicalService.SpecialtyId;
        existingMedicalService.PrivatePrice = medicalService.PrivatePrice;

        await _context.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var medicalService = await _context.MedicalServices
            .FirstOrDefaultAsync(s => s.Id == id);

        if (medicalService is null)
        {
            throw new BusinessException("La prestación médica no existe.");
        }

        if (!medicalService.IsActive)
        {
            throw new BusinessException(
                "La prestación médica ya se encuentra inactiva.");
        }

        medicalService.IsActive = false;

        await _context.SaveChangesAsync();
    }

    public async Task ActivateAsync(int id)
    {
        var medicalService = await _context.MedicalServices
            .FirstOrDefaultAsync(s => s.Id == id);

        if (medicalService is null)
        {
            throw new BusinessException("La prestación médica no existe.");
        }

        if (medicalService.IsActive)
        {
            throw new BusinessException(
                "La prestación médica ya se encuentra activa.");
        }

        medicalService.IsActive = true;

        await _context.SaveChangesAsync();
    }

    private async Task ValidateSpecialtyAsync(int? specialtyId)
    {
        if (!specialtyId.HasValue)
            return;

        var specialty = await _context.Specialties
            .FirstOrDefaultAsync(s => s.Id == specialtyId.Value);

        if (specialty is null)
        {
            throw new BusinessException(
                "La especialidad seleccionada no existe.");
        }

        if (!specialty.IsActive)
        {
            throw new BusinessException(
                "No se pueden asociar prestaciones a una especialidad inactiva.");
        }
    }

    private static void ValidateMedicalService(MedicalService medicalService)
    {
        if (string.IsNullOrWhiteSpace(medicalService.Name))
        {
            throw new BusinessException(
                "El nombre de la prestación es obligatorio.");
        }

        if (medicalService.PrivatePrice <= 0)
        {
            throw new BusinessException(
                "El valor particular debe ser mayor a cero.");
        }

        if (medicalService.PrivatePrice > 999999999999.99m)
        {
            throw new BusinessException(
                "El valor particular supera el importe máximo permitido.");
        }
    }

    private static void NormalizeMedicalServiceData(
        MedicalService medicalService)
    {
        medicalService.Code = Normalize(medicalService.Code);
        medicalService.Name = medicalService.Name?.Trim() ?? string.Empty;
        medicalService.Description = Normalize(medicalService.Description);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}
