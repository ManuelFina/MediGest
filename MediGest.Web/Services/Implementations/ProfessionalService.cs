
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services.Implementations;

public class ProfessionalService : IProfessionalService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Models.Identity.ApplicationUser> _userManager;

    public ProfessionalService(
        ApplicationDbContext context,
        UserManager<Models.Identity.ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IEnumerable<Professional>> GetAllAsync()
    {
        return await _context.Professionals
            .AsNoTracking()
            .Include(p => p.ApplicationUser)
            .OrderBy(p => p.ApplicationUser.LastName)
            .ThenBy(p => p.ApplicationUser.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Professional>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync();
        }

        searchTerm = searchTerm.Trim();

        return await _context.Professionals
            .AsNoTracking()
            .Include(p => p.ApplicationUser)
            .Where(p =>
                p.ApplicationUser.FirstName.Contains(searchTerm) ||
                p.ApplicationUser.LastName.Contains(searchTerm) ||
                p.LicenseNumber.Contains(searchTerm))
            .OrderBy(p => p.ApplicationUser.LastName)
            .ThenBy(p => p.ApplicationUser.FirstName)
            .ToListAsync();
    }

    public async Task<Professional?> GetByIdAsync(int id)
    {
        return await _context.Professionals
            .AsNoTracking()
            .Include(p => p.ApplicationUser)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Professional?> GetByApplicationUserIdAsync(
        string applicationUserId)
    {
        if (string.IsNullOrWhiteSpace(applicationUserId))
        {
            return null;
        }

        return await _context.Professionals
            .AsNoTracking()
            .Include(p => p.ApplicationUser)
            .FirstOrDefaultAsync(p => p.ApplicationUserId == applicationUserId);
    }

    public async Task<Professional> CreateAsync(Professional professional)
    {
        NormalizeProfessionalData(professional);

        await ValidateProfessionalAsync(professional);

        var licenseExists = await _context.Professionals
            .AnyAsync(p => p.LicenseNumber == professional.LicenseNumber);

        if (licenseExists)
        {
            throw new BusinessException(
                "Ya existe un profesional registrado con esa matrícula.");
        }

        var userExists = await _context.Professionals
            .AnyAsync(p => p.ApplicationUserId == professional.ApplicationUserId);

        if (userExists)
        {
            throw new BusinessException(
                "Este usuario ya está vinculado a un profesional.");
        }

        var user = await _userManager.FindByIdAsync(
            professional.ApplicationUserId);

        if (user is null)
        {
            throw new BusinessException("El usuario seleccionado no existe.");
        }

        var hasDoctorRole = await _userManager.IsInRoleAsync(user, "Doctor");

        if (!hasDoctorRole)
        {
            throw new BusinessException(
                "El usuario seleccionado no tiene el rol de profesional médico.");
        }

        professional.IsActive = true;

        _context.Professionals.Add(professional);

        await _context.SaveChangesAsync();

        return professional;
    }

    public async Task UpdateAsync(Professional professional)
    {
        NormalizeProfessionalData(professional);

        var existingProfessional = await _context.Professionals
            .FirstOrDefaultAsync(p => p.Id == professional.Id);

        if (existingProfessional is null)
        {
            throw new BusinessException("El profesional no existe.");
        }

        await ValidateProfessionalAsync(professional);

        var licenseExists = await _context.Professionals
            .AnyAsync(p =>
                p.LicenseNumber == professional.LicenseNumber &&
                p.Id != professional.Id);

        if (licenseExists)
        {
            throw new BusinessException(
                "Ya existe otro profesional registrado con esa matrícula.");
        }

        if (existingProfessional.ApplicationUserId != professional.ApplicationUserId)
        {
            throw new BusinessException(
                "No se puede cambiar el usuario vinculado a un profesional.");
        }

        existingProfessional.LicenseNumber = professional.LicenseNumber;

        await _context.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var professional = await _context.Professionals
            .FirstOrDefaultAsync(p => p.Id == id);

        if (professional is null)
        {
            throw new BusinessException("El profesional no existe.");
        }

        if (!professional.IsActive)
        {
            throw new BusinessException(
                "El profesional ya se encuentra inactivo.");
        }

        professional.IsActive = false;

        await _context.SaveChangesAsync();
    }

    public async Task ActivateAsync(int id)
    {
        var professional = await _context.Professionals
            .FirstOrDefaultAsync(p => p.Id == id);

        if (professional is null)
        {
            throw new BusinessException("El profesional no existe.");
        }

        if (professional.IsActive)
        {
            throw new BusinessException(
                "El profesional ya se encuentra activo.");
        }

        professional.IsActive = true;

        await _context.SaveChangesAsync();
    }

    private static void NormalizeProfessionalData(Professional professional)
    {
        professional.ApplicationUserId =
            professional.ApplicationUserId?.Trim() ?? string.Empty;

        professional.LicenseNumber =
            professional.LicenseNumber?.Trim() ?? string.Empty;
    }

    private static Task ValidateProfessionalAsync(Professional professional)
    {
        if (string.IsNullOrWhiteSpace(professional.ApplicationUserId))
        {
            throw new BusinessException(
                "Debe seleccionar un usuario.");
        }

        if (string.IsNullOrWhiteSpace(professional.LicenseNumber))
        {
            throw new BusinessException(
                "La matrícula profesional es obligatoria.");
        }

        return Task.CompletedTask;
    }
}
