
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Enums;
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
            .AnyAsync(p =>
                p.Id == schedule.ProfessionalId &&
                p.IsActive);

        if (!professionalExists)
        {
            throw new BusinessException(
                "El profesional no existe o se encuentra inactivo.");
        }

        await ValidateOverlappingSchedulesAsync(schedule);

        schedule.ScheduleDays = new List<ScheduleDay>();

        _context.ProfessionalSchedules.Add(schedule);

        await _context.SaveChangesAsync();

        return schedule;
    }

    public async Task UpdateAsync(ProfessionalSchedule schedule)
    {
        ValidateScheduleDates(schedule);
        ValidateAppointmentDuration(schedule);

        var existingSchedule = await _context.ProfessionalSchedules
            .FirstOrDefaultAsync(s => s.Id == schedule.Id);

        if (existingSchedule is null)
        {
            throw new BusinessException(
                "La agenda no existe.");
        }

        if (existingSchedule.ProfessionalId != schedule.ProfessionalId)
        {
            throw new BusinessException(
                "No se puede cambiar el profesional asociado a una agenda.");
        }

        await ValidateOverlappingSchedulesAsync(schedule);

        await ValidateExistingAppointmentsAsync(schedule);

        existingSchedule.Name = schedule.Name;
        existingSchedule.StartDate = schedule.StartDate;
        existingSchedule.EndDate = schedule.EndDate;
        existingSchedule.AppointmentDurationMinutes =
            schedule.AppointmentDurationMinutes;
        existingSchedule.IsActive = schedule.IsActive;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var schedule = await _context.ProfessionalSchedules
            .Include(s => s.ScheduleDays)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (schedule is null)
        {
            throw new BusinessException(
                "La agenda no existe.");
        }

        var hasAppointments = await _context.Appointments
            .AnyAsync(a =>
                a.ProfessionalId == schedule.ProfessionalId &&
                a.AppointmentDate >= schedule.StartDate &&
                a.AppointmentDate <= schedule.EndDate &&
                a.Status != AppointmentStatus.Cancelled);

        if (hasAppointments)
        {
            throw new BusinessException(
                "No se puede eliminar la agenda porque tiene turnos asignados.");
        }

        _context.ProfessionalSchedules.Remove(schedule);

        await _context.SaveChangesAsync();
    }

    private async Task ValidateOverlappingSchedulesAsync(
        ProfessionalSchedule schedule)
    {
        // Las agendas inactivas no bloquean períodos.
        if (!schedule.IsActive)
        {
            return;
        }

        var hasOverlap = await _context.ProfessionalSchedules
            .AnyAsync(s =>
                s.ProfessionalId == schedule.ProfessionalId &&
                s.IsActive &&
                s.Id != schedule.Id &&
                schedule.StartDate <= s.EndDate &&
                schedule.EndDate >= s.StartDate);

        if (hasOverlap)
        {
            throw new BusinessException(
                "El período de la agenda se superpone con otra agenda activa del profesional.");
        }
    }

    private async Task ValidateExistingAppointmentsAsync(
        ProfessionalSchedule schedule)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(a =>
                a.ProfessionalId == schedule.ProfessionalId &&
                a.Status != AppointmentStatus.Cancelled &&
                a.AppointmentDate >= today &&
                (a.AppointmentDate < schedule.StartDate ||
                 a.AppointmentDate > schedule.EndDate))
            .ToListAsync();

        if (appointments.Count == 0)
        {
            return;
        }

        var otherActiveSchedules = await _context.ProfessionalSchedules
            .AsNoTracking()
            .Where(s =>
                s.Id != schedule.Id &&
                s.ProfessionalId == schedule.ProfessionalId &&
                s.IsActive)
            .Select(s => new
            {
                s.StartDate,
                s.EndDate
            })
            .ToListAsync();

        var hasAppointmentsOutsideAvailablePeriods = appointments.Any(
            appointment =>
                !otherActiveSchedules.Any(other =>
                    appointment.AppointmentDate >= other.StartDate &&
                    appointment.AppointmentDate <= other.EndDate));

        if (hasAppointmentsOutsideAvailablePeriods)
        {
            throw new BusinessException(
                "No se puede modificar el período de la agenda porque existen turnos asignados fuera del nuevo rango de fechas.");
        }
    }

    private static void ValidateScheduleDates(
        ProfessionalSchedule schedule)
    {
        if (schedule.StartDate == default ||
            schedule.EndDate == default ||
            schedule.EndDate < schedule.StartDate)
        {
            throw new BusinessException(
                "El período de la agenda no es válido.");
        }
    }

    private static void ValidateAppointmentDuration(
        ProfessionalSchedule schedule)
    {
        if (schedule.AppointmentDurationMinutes < 5 ||
            schedule.AppointmentDurationMinutes > 240)
        {
            throw new BusinessException(
                "La duración del turno debe estar entre 5 y 240 minutos.");
        }
    }
}
