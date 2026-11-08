
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services.Implementations;

public class ProfessionalScheduleService : IProfessionalScheduleService
{
    private readonly ApplicationDbContext _context;

    public ProfessionalScheduleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProfessionalSchedule>> GetByProfessionalAsync(
        int professionalId)
    {
        return await _context.ProfessionalSchedules
            .AsNoTracking()
            .Include(s => s.ScheduleDays)
            .Where(s => s.ProfessionalId == professionalId)
            .OrderBy(s => s.StartDate)
            .ToListAsync();
    }

    public async Task<ProfessionalSchedule?> GetByIdAsync(int id)
    {
        return await _context.ProfessionalSchedules
            .AsNoTracking()
            .Include(s => s.ScheduleDays)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<ProfessionalSchedule> CreateAsync(
        ProfessionalSchedule schedule)
    {
        ValidateScheduleDates(schedule);
        ValidateAppointmentDuration(schedule);

        var professionalExists = await _context.Professionals
            .AnyAsync(p => p.Id == schedule.ProfessionalId && p.IsActive);

        if (!professionalExists)
        {
            throw new BusinessException(
                "El profesional no existe o se encuentra inactivo.");
        }

        var hasOverlappingSchedule = await _context.ProfessionalSchedules
            .AnyAsync(s =>
                s.ProfessionalId == schedule.ProfessionalId &&
                schedule.StartDate <= s.EndDate &&
                schedule.EndDate >= s.StartDate);

        if (hasOverlappingSchedule)
        {
            throw new BusinessException(
                "El profesional ya tiene una agenda configurada para ese período.");
        }

        _context.ProfessionalSchedules.Add(schedule);

        await _context.SaveChangesAsync();

        return schedule;
    }

    public async Task UpdateAsync(ProfessionalSchedule schedule)
    {
        ValidateScheduleDates(schedule);
        ValidateAppointmentDuration(schedule);

        var existingSchedule = await _context.ProfessionalSchedules
            .Include(s => s.ScheduleDays)
            .FirstOrDefaultAsync(s => s.Id == schedule.Id);

        if (existingSchedule is null)
        {
            throw new BusinessException(
                "No se encontró la agenda que intentás modificar.");
        }

        var hasOverlappingSchedule = await _context.ProfessionalSchedules
            .AnyAsync(s =>
                s.Id != schedule.Id &&
                s.ProfessionalId == schedule.ProfessionalId &&
                schedule.StartDate <= s.EndDate &&
                schedule.EndDate >= s.StartDate);

        if (hasOverlappingSchedule)
        {
            throw new BusinessException(
                "El profesional ya tiene otra agenda configurada para ese período.");
        }

        existingSchedule.Name = schedule.Name;
        existingSchedule.StartDate = schedule.StartDate;
        existingSchedule.EndDate = schedule.EndDate;
        existingSchedule.AppointmentDurationMinutes =
            schedule.AppointmentDurationMinutes;
        existingSchedule.IsActive = schedule.IsActive;
        existingSchedule.ProfessionalId = schedule.ProfessionalId;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var schedule = await _context.ProfessionalSchedules
            .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule is null)
        {
            throw new BusinessException(
                "No se encontró la agenda que intentás eliminar.");
        }

        var hasAppointments = await _context.Appointments
            .AnyAsync(a =>
                a.ProfessionalId == schedule.ProfessionalId &&
                a.AppointmentDate >= schedule.StartDate &&
                a.AppointmentDate <= schedule.EndDate);

        if (hasAppointments)
        {
            throw new BusinessException(
                "No se puede eliminar la agenda porque existen turnos asociados a ese período.");
        }

        _context.ProfessionalSchedules.Remove(schedule);

        await _context.SaveChangesAsync();
    }

    private static void ValidateScheduleDates(
        ProfessionalSchedule schedule)
    {
        if (schedule.StartDate == default)
        {
            throw new BusinessException(
                "La fecha de inicio es obligatoria.");
        }

        if (schedule.EndDate <= schedule.StartDate)
        {
            throw new BusinessException(
                "La fecha de finalización debe ser posterior a la fecha de inicio.");
        }
    }

    private static void ValidateAppointmentDuration(
        ProfessionalSchedule schedule)
    {
        if (schedule.AppointmentDurationMinutes is < 5 or > 240)
        {
            throw new BusinessException(
                "La duración del turno debe estar entre 5 y 240 minutos.");
        }
    }
}
