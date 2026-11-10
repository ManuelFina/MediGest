
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services;

public class SpecialtyService : ISpecialtyService
{
    private readonly ApplicationDbContext _context;

    public SpecialtyService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Specialty>> GetAllAsync()
    {
        return await _context.Specialties
            .AsNoTracking()
            .Include(s => s.Professionals)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<IEnumerable<Specialty>> GetActiveAsync()
    {
        return await _context.Specialties
            .AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Specialty?> GetByIdAsync(int id)
    {
        return await _context.Specialties
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Specialty?> GetByNameAsync(string name)
    {
        string normalizedName = name.Trim();

        return await _context.Specialties
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Name == normalizedName);
    }

    public async Task<Specialty> CreateAsync(Specialty specialty)
    {
        string name = specialty.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("El nombre de la especialidad es obligatorio.");

        if (await _context.Specialties.AnyAsync(s => s.Name == name))
            throw new BusinessException("Ya existe una especialidad con ese nombre.");

        specialty.Name = name;
        specialty.Description = string.IsNullOrWhiteSpace(specialty.Description)
            ? null
            : specialty.Description.Trim();

        specialty.IsActive = true;

        _context.Specialties.Add(specialty);
        await _context.SaveChangesAsync();

        return specialty;
    }

    public async Task UpdateAsync(int id, Specialty specialty)
    {
        var existingSpecialty = await _context.Specialties
            .FirstOrDefaultAsync(s => s.Id == id);

        if (existingSpecialty is null)
            throw new BusinessException("La especialidad no existe.");

        string name = specialty.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("El nombre de la especialidad es obligatorio.");

        bool duplicateName = await _context.Specialties
            .AnyAsync(s => s.Id != id && s.Name == name);

        if (duplicateName)
            throw new BusinessException("Ya existe otra especialidad con ese nombre.");

        existingSpecialty.Name = name;
        existingSpecialty.Description = string.IsNullOrWhiteSpace(specialty.Description)
            ? null
            : specialty.Description.Trim();

        await _context.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var specialty = await _context.Specialties
            .Include(s => s.Professionals)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (specialty is null)
            throw new BusinessException("La especialidad no existe.");

        if (!specialty.IsActive)
            throw new BusinessException("La especialidad ya está desactivada.");

        if (specialty.Professionals.Any(p => p.IsActive))
            throw new BusinessException(
                "No se puede desactivar esta especialidad porque tiene médicos activos asociados.");

        specialty.IsActive = false;

        await _context.SaveChangesAsync();
    }

    public async Task ActivateAsync(int id)
    {
        var specialty = await _context.Specialties
            .FirstOrDefaultAsync(s => s.Id == id);

        if (specialty is null)
            throw new BusinessException("La especialidad no existe.");

        if (specialty.IsActive)
            throw new BusinessException("La especialidad ya está activa.");

        specialty.IsActive = true;

        await _context.SaveChangesAsync();
    }
}
