
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services.Implementations;

public class PatientService : IPatientService
{
    private readonly ApplicationDbContext _context;

    public PatientService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Patient>> GetAllAsync()
    {
        return await _context.Patients
            .AsNoTracking()
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Patient>> SearchAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllAsync();
        }

        searchTerm = searchTerm.Trim();

        return await _context.Patients
            .AsNoTracking()
            .Where(p =>
                p.FirstName.Contains(searchTerm) ||
                p.LastName.Contains(searchTerm) ||
                p.DocumentNumber.Contains(searchTerm))
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();
    }

    public async Task<Patient?> GetByIdAsync(int id)
    {
        return await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Patient?> GetByDocumentNumberAsync(string documentNumber)
    {
        if (string.IsNullOrWhiteSpace(documentNumber))
        {
            return null;
        }

        documentNumber = documentNumber.Trim();

        return await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.DocumentNumber == documentNumber);
    }

    public async Task<Patient> CreateAsync(Patient patient)
    {
        NormalizePatientData(patient);

        await ValidatePatientAsync(patient);

        var documentExists = await _context.Patients
            .AnyAsync(p => p.DocumentNumber == patient.DocumentNumber);

        if (documentExists)
        {
            throw new BusinessException(
                "Ya existe un paciente registrado con ese documento.");
        }

        patient.IsActive = true;

        _context.Patients.Add(patient);

        await _context.SaveChangesAsync();

        return patient;
    }

    public async Task UpdateAsync(Patient patient)
    {
        NormalizePatientData(patient);

        var existingPatient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == patient.Id);

        if (existingPatient is null)
        {
            throw new BusinessException("El paciente no existe.");
        }

        await ValidatePatientAsync(patient);

        var documentExists = await _context.Patients
            .AnyAsync(p =>
                p.DocumentNumber == patient.DocumentNumber &&
                p.Id != patient.Id);

        if (documentExists)
        {
            throw new BusinessException(
                "Ya existe otro paciente registrado con ese documento.");
        }

        existingPatient.FirstName = patient.FirstName;
        existingPatient.LastName = patient.LastName;
        existingPatient.DocumentNumber = patient.DocumentNumber;
        existingPatient.DateOfBirth = patient.DateOfBirth;
        existingPatient.PhoneNumber = patient.PhoneNumber;
        existingPatient.Email = patient.Email;
        existingPatient.Address = patient.Address;

        await _context.SaveChangesAsync();
    }

    public async Task DeactivateAsync(int id)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient is null)
        {
            throw new BusinessException("El paciente no existe.");
        }

        if (!patient.IsActive)
        {
            throw new BusinessException(
                "El paciente ya se encuentra inactivo.");
        }

        patient.IsActive = false;

        await _context.SaveChangesAsync();
    }

    public async Task ActivateAsync(int id)
    {
        var patient = await _context.Patients
            .FirstOrDefaultAsync(p => p.Id == id);

        if (patient is null)
        {
            throw new BusinessException("El paciente no existe.");
        }

        if (patient.IsActive)
        {
            throw new BusinessException(
                "El paciente ya se encuentra activo.");
        }

        patient.IsActive = true;

        await _context.SaveChangesAsync();
    }

    private static void NormalizePatientData(Patient patient)
    {
        patient.FirstName = patient.FirstName?.Trim() ?? string.Empty;
        patient.LastName = patient.LastName?.Trim() ?? string.Empty;
        patient.DocumentNumber = patient.DocumentNumber?.Trim() ?? string.Empty;
        patient.PhoneNumber = string.IsNullOrWhiteSpace(patient.PhoneNumber)
            ? null
            : patient.PhoneNumber.Trim();
        patient.Email = string.IsNullOrWhiteSpace(patient.Email)
            ? null
            : patient.Email.Trim();
        patient.Address = string.IsNullOrWhiteSpace(patient.Address)
            ? null
            : patient.Address.Trim();
    }

    private static Task ValidatePatientAsync(Patient patient)
    {
        if (string.IsNullOrWhiteSpace(patient.FirstName))
        {
            throw new BusinessException(
                "El nombre del paciente es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(patient.LastName))
        {
            throw new BusinessException(
                "El apellido del paciente es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(patient.DocumentNumber))
        {
            throw new BusinessException(
                "El documento del paciente es obligatorio.");
        }

        if (patient.DateOfBirth == default)
        {
            throw new BusinessException(
                "La fecha de nacimiento es obligatoria.");
        }

        if (patient.DateOfBirth > DateOnly.FromDateTime(DateTime.Today))
        {
            throw new BusinessException(
                "La fecha de nacimiento no puede ser futura.");
        }

        return Task.CompletedTask;
    }
}
