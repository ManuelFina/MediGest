
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Enums;
using MediGest.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services.Implementations;

public class ScheduleExceptionService : IScheduleExceptionService
{
    private readonly ApplicationDbContext _context;

    public ScheduleExceptionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ScheduleException>> GetByProfessionalAsync(
        int professionalId)
    {
        return await _context.ScheduleExceptions
            .AsNoTracking()
            .Where(e => e.ProfessionalId == professionalId)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<ScheduleException?> GetByIdAsync(int id)
    {
        return await _context.ScheduleExceptions
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<ScheduleException> CreateAsync(
        ScheduleException scheduleException)
    {
        await ValidateExceptionAsync(scheduleException);

        _context.ScheduleExceptions.Add(scheduleException);

        await _context.SaveChangesAsync();

        return scheduleException;
    }

    public async Task UpdateAsync(ScheduleException scheduleException)
    {
        var existingException = await _context.ScheduleExceptions
            .FirstOrDefaultAsync(e => e.Id == scheduleException.Id);

        if (existingException is null)
        {
            throw new BusinessException(
                "No se encontró la excepción que intentás modificar.");
        }

        if (existingException.ProfessionalId != scheduleException.ProfessionalId)
        {
            throw new BusinessException(
                "No se puede cambiar el profesional asociado a una excepción.");
        }

        await ValidateExceptionAsync(
            scheduleException,
            existingException.Id);

        existingException.StartDate = scheduleException.StartDate;
        existingException.EndDate = scheduleException.EndDate;
        existingException.StartTime = scheduleException.StartTime;
        existingException.EndTime = scheduleException.EndTime;
        existingException.Reason = scheduleException.Reason.Trim();

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var scheduleException = await _context.ScheduleExceptions
            .FirstOrDefaultAsync(e => e.Id == id);

        if (scheduleException is null)
        {
            throw new BusinessException(
                "No se encontró la excepción que intentás eliminar.");
        }

        _context.ScheduleExceptions.Remove(scheduleException);

        await _context.SaveChangesAsync();
    }

    private async Task ValidateExceptionAsync(
        ScheduleException scheduleException,
        int? excludedId = null)
    {
        if (scheduleException.StartDate == default ||
            scheduleException.EndDate == default ||
            scheduleException.EndDate < scheduleException.StartDate)
        {
            throw new BusinessException(
                "El rango de fechas de la excepción no es válido.");
        }

        if (scheduleException.StartTime.HasValue !=
            scheduleException.EndTime.HasValue)
        {
            throw new BusinessException(
                "Debés indicar tanto la hora de inicio como la de finalización.");
        }

        if (scheduleException.StartTime.HasValue &&
            scheduleException.StartTime.Value >= scheduleException.EndTime!.Value)
        {
            throw new BusinessException(
                "La hora de finalización debe ser posterior a la hora de inicio.");
        }

        if (string.IsNullOrWhiteSpace(scheduleException.Reason))
        {
            throw new BusinessException(
                "El motivo de la excepción es obligatorio.");
        }

        scheduleException.Reason = scheduleException.Reason.Trim();

        if (scheduleException.Reason.Length > 250)
        {
            throw new BusinessException(
                "El motivo de la excepción no puede superar los 250 caracteres.");
        }

        var professionalExists = await _context.Professionals
            .AnyAsync(p =>
                p.Id == scheduleException.ProfessionalId &&
                p.IsActive);

        if (!professionalExists)
        {
            throw new BusinessException(
                "El profesional no existe o se encuentra inactivo.");
        }

        var hasOverlappingException = await _context.ScheduleExceptions
            .AnyAsync(e =>
                e.ProfessionalId == scheduleException.ProfessionalId &&
                (!excludedId.HasValue || e.Id != excludedId.Value) &&
                scheduleException.StartDate <= e.EndDate &&
                scheduleException.EndDate >= e.StartDate &&
                (
                    !scheduleException.StartTime.HasValue ||
                    !e.StartTime.HasValue ||
                    (
                        scheduleException.StartTime.Value < e.EndTime!.Value &&
                        scheduleException.EndTime!.Value > e.StartTime.Value
                    )
                ));

        if (hasOverlappingException)
        {
            throw new BusinessException(
                "La excepción se superpone con otra excepción existente del profesional.");
        }

        var hasAppointments = await _context.Appointments
            .AnyAsync(a =>
                a.ProfessionalId == scheduleException.ProfessionalId &&
                a.AppointmentDate >= scheduleException.StartDate &&
                a.AppointmentDate <= scheduleException.EndDate &&
                a.Status != AppointmentStatus.Cancelled &&
                (
                    !scheduleException.StartTime.HasValue ||
                    (
                        a.StartTime < scheduleException.EndTime!.Value &&
                        a.EndTime > scheduleException.StartTime.Value
                    )
                ));

        if (hasAppointments)
        {
            throw new BusinessException(
                "No se puede registrar la excepción porque existen turnos asignados en ese período.");
        }
    }
}
