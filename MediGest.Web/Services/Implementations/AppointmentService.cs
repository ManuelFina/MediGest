
using System.Data;
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Enums;
using MediGest.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services.Implementations;

public class AppointmentService : IAppointmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IAppointmentAvailabilityService _availabilityService;

    public AppointmentService(
        ApplicationDbContext context,
        IAppointmentAvailabilityService availabilityService)
    {
        _context = context;
        _availabilityService = availabilityService;
    }

    public async Task<IEnumerable<Appointment>> GetAllAsync()
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Professional)
                .ThenInclude(p => p.ApplicationUser)
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByDateAsync(DateOnly date)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Professional)
                .ThenInclude(p => p.ApplicationUser)
            .Where(a => a.AppointmentDate == date)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByProfessionalAsync(
        int professionalId,
        DateOnly? date = null)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Professional)
                .ThenInclude(p => p.ApplicationUser)
            .Where(a => a.ProfessionalId == professionalId);

        if (date.HasValue)
        {
            query = query.Where(a => a.AppointmentDate == date.Value);
        }

        return await query
            .OrderBy(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Professional)
                .ThenInclude(p => p.ApplicationUser)
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _context.Appointments
            .AsNoTracking()
            .Include(a => a.Patient)
            .Include(a => a.Professional)
                .ThenInclude(p => p.ApplicationUser)
            .Include(a => a.History)
                .ThenInclude(h => h.ChangedByUser)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Appointment> CreateAsync(
        Appointment appointment,
        string changedByUserId)
    {
        ValidateUserId(changedByUserId);

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var patient = await _context.Patients
            .FirstOrDefaultAsync(p =>
                p.Id == appointment.PatientId && p.IsActive);

        if (patient is null)
        {
            throw new BusinessException(
                "El paciente no existe o se encuentra inactivo.");
        }

        var professional = await _context.Professionals
            .FirstOrDefaultAsync(p =>
                p.Id == appointment.ProfessionalId && p.IsActive);

        if (professional is null)
        {
            throw new BusinessException(
                "El profesional no existe o se encuentra inactivo.");
        }

        var schedule = await GetActiveScheduleAsync(
            appointment.ProfessionalId,
            appointment.AppointmentDate);

        if (schedule is null)
        {
            throw new BusinessException(
                "El profesional no tiene una agenda activa para esa fecha.");
        }

        var availableSlots =
            await _availabilityService.GetAvailableSlotsAsync(
                appointment.ProfessionalId,
                appointment.AppointmentDate);

        if (!availableSlots.Contains(appointment.StartTime))
        {
            throw new BusinessException(
                "El horario seleccionado ya no está disponible.");
        }

        appointment.EndTime = appointment.StartTime.AddMinutes(
            schedule.AppointmentDurationMinutes);

        await ValidatePatientAvailabilityAsync(
            appointment.PatientId,
            appointment.AppointmentDate,
            appointment.StartTime,
            appointment.EndTime);

        appointment.Status = AppointmentStatus.Pending;
        appointment.CreatedAt = DateTime.UtcNow;
        appointment.UpdatedAt = null;

        appointment.History.Add(new AppointmentHistory
        {
            Description = "Turno creado.",
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = changedByUserId
        });

        _context.Appointments.Add(appointment);

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return appointment;
    }

    public async Task ConfirmAsync(int id, string changedByUserId)
    {
        await ChangeStatusAsync(
            id,
            AppointmentStatus.Confirmed,
            "Turno confirmado.",
            changedByUserId,
            AppointmentStatus.Pending);
    }

    public async Task CancelAsync(
        int id,
        string reason,
        string changedByUserId)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessException(
                "Debe indicar el motivo de cancelación.");
        }

        await ChangeStatusAsync(
            id,
            AppointmentStatus.Cancelled,
            $"Turno cancelado. Motivo: {reason.Trim()}",
            changedByUserId,
            AppointmentStatus.Pending,
            AppointmentStatus.Confirmed);
    }

    public async Task CompleteAsync(int id, string changedByUserId)
    {
        await ChangeStatusAsync(
            id,
            AppointmentStatus.Completed,
            "Turno marcado como completado.",
            changedByUserId,
            AppointmentStatus.Confirmed);
    }

    public async Task MarkAsNoShowAsync(int id, string changedByUserId)
    {
        await ChangeStatusAsync(
            id,
            AppointmentStatus.NoShow,
            "Paciente ausente.",
            changedByUserId,
            AppointmentStatus.Confirmed);
    }

    public async Task RescheduleAsync(
        int id,
        DateOnly newDate,
        TimeOnly newStartTime,
        string changedByUserId)
    {
        ValidateUserId(changedByUserId);

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment is null)
        {
            throw new BusinessException("El turno no existe.");
        }

        if (appointment.Status != AppointmentStatus.Pending &&
            appointment.Status != AppointmentStatus.Confirmed)
        {
            throw new BusinessException(
                "No se puede reprogramar un turno que ya fue cancelado, completado o marcado como ausente.");
        }

        if (appointment.AppointmentDate == newDate &&
            appointment.StartTime == newStartTime)
        {
            await transaction.CommitAsync();
            return;
        }

        var schedule = await GetActiveScheduleAsync(
            appointment.ProfessionalId,
            newDate);

        if (schedule is null)
        {
            throw new BusinessException(
                "El profesional no tiene una agenda activa para esa fecha.");
        }

        var availableSlots =
            await _availabilityService.GetAvailableSlotsAsync(
                appointment.ProfessionalId,
                newDate);

        if (!availableSlots.Contains(newStartTime))
        {
            throw new BusinessException(
                "El nuevo horario seleccionado no está disponible.");
        }

        var newEndTime = newStartTime.AddMinutes(
            schedule.AppointmentDurationMinutes);

        await ValidatePatientAvailabilityAsync(
            appointment.PatientId,
            newDate,
            newStartTime,
            newEndTime,
            appointment.Id);

        var oldDate = appointment.AppointmentDate;
        var oldStartTime = appointment.StartTime;

        appointment.AppointmentDate = newDate;
        appointment.StartTime = newStartTime;
        appointment.EndTime = newEndTime;
        appointment.UpdatedAt = DateTime.UtcNow;

        appointment.History.Add(new AppointmentHistory
        {
            Description =
                $"Turno reprogramado de {oldDate:dd/MM/yyyy} {oldStartTime:HH:mm} a {newDate:dd/MM/yyyy} {newStartTime:HH:mm}.",
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = changedByUserId
        });

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private async Task ChangeStatusAsync(
        int id,
        AppointmentStatus newStatus,
        string description,
        string changedByUserId,
        params AppointmentStatus[] allowedStatuses)
    {
        ValidateUserId(changedByUserId);

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment is null)
        {
            throw new BusinessException("El turno no existe.");
        }

        if (!allowedStatuses.Contains(appointment.Status))
        {
            throw new BusinessException(
                "No se puede realizar esta operación con el estado actual del turno.");
        }

        appointment.Status = newStatus;
        appointment.UpdatedAt = DateTime.UtcNow;

        appointment.History.Add(new AppointmentHistory
        {
            Description = description,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = changedByUserId
        });

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private async Task<ProfessionalSchedule?> GetActiveScheduleAsync(
        int professionalId,
        DateOnly date)
    {
        return await _context.ProfessionalSchedules
            .Include(s => s.ScheduleDays)
            .FirstOrDefaultAsync(s =>
                s.ProfessionalId == professionalId &&
                s.IsActive &&
                s.StartDate <= date &&
                s.EndDate >= date);
    }

    private async Task ValidatePatientAvailabilityAsync(
        int patientId,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        int? excludedAppointmentId = null)
    {
        var hasConflict = await _context.Appointments.AnyAsync(a =>
            a.PatientId == patientId &&
            a.AppointmentDate == date &&
            a.Status != AppointmentStatus.Cancelled &&
            (!excludedAppointmentId.HasValue ||
             a.Id != excludedAppointmentId.Value) &&
            startTime < a.EndTime &&
            endTime > a.StartTime);

        if (hasConflict)
        {
            throw new BusinessException(
                "El paciente ya tiene otro turno en ese horario.");
        }
    }

    private static void ValidateUserId(string changedByUserId)
    {
        if (string.IsNullOrWhiteSpace(changedByUserId))
        {
            throw new BusinessException(
                "No se pudo identificar al usuario que realiza la operación.");
        }
    }
}
