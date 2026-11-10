
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Enums;
using MediGest.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services.Implementations;

public class ScheduleDayService : IScheduleDayService
{
    private readonly ApplicationDbContext _context;

    public ScheduleDayService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ScheduleDay>> GetByScheduleAsync(
        int scheduleId)
    {
        return await _context.ScheduleDays
            .AsNoTracking()
            .Where(d => d.ProfessionalScheduleId == scheduleId)
            .OrderBy(d => d.DayOfWeek)
            .ThenBy(d => d.StartTime)
            .ToListAsync();
    }

    public async Task<ScheduleDay> CreateAsync(ScheduleDay scheduleDay)
    {
        await ValidateScheduleDayAsync(scheduleDay);

        _context.ScheduleDays.Add(scheduleDay);

        await _context.SaveChangesAsync();

        return scheduleDay;
    }

    public async Task UpdateAsync(ScheduleDay scheduleDay)
    {
        var existingDay = await _context.ScheduleDays
            .FirstOrDefaultAsync(d => d.Id == scheduleDay.Id);

        if (existingDay is null)
        {
            throw new BusinessException(
                "El bloque horario no existe.");
        }

        // El bloque no puede trasladarse a otra agenda.
        scheduleDay.ProfessionalScheduleId =
            existingDay.ProfessionalScheduleId;

        await ValidateScheduleDayAsync(scheduleDay);

        await ValidateExistingAppointmentsAsync(
            existingDay,
            scheduleDay);

        existingDay.DayOfWeek = scheduleDay.DayOfWeek;
        existingDay.StartTime = scheduleDay.StartTime;
        existingDay.EndTime = scheduleDay.EndTime;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var scheduleDay = await _context.ScheduleDays
            .FirstOrDefaultAsync(d => d.Id == id);

        if (scheduleDay is null)
        {
            throw new BusinessException(
                "El bloque horario no existe.");
        }

        await ValidateExistingAppointmentsAsync(
            scheduleDay,
            null);

        _context.ScheduleDays.Remove(scheduleDay);

        await _context.SaveChangesAsync();
    }

    private async Task ValidateScheduleDayAsync(
        ScheduleDay scheduleDay)
    {
        if (scheduleDay.StartTime >= scheduleDay.EndTime)
        {
            throw new BusinessException(
                "La hora de inicio debe ser anterior a la hora de finalización.");
        }

        if (!Enum.IsDefined(scheduleDay.DayOfWeek))
        {
            throw new BusinessException(
                "El día de la semana seleccionado no es válido.");
        }

        var schedule = await _context.ProfessionalSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == scheduleDay.ProfessionalScheduleId);

        if (schedule is null)
        {
            throw new BusinessException(
                "La agenda seleccionada no existe.");
        }

        var hasOverlap = await _context.ScheduleDays
            .AnyAsync(d =>
                d.ProfessionalScheduleId ==
                    scheduleDay.ProfessionalScheduleId &&
                d.DayOfWeek == scheduleDay.DayOfWeek &&
                d.Id != scheduleDay.Id &&
                scheduleDay.StartTime < d.EndTime &&
                scheduleDay.EndTime > d.StartTime);

        if (hasOverlap)
        {
            throw new BusinessException(
                "El bloque horario se superpone con otro bloque del mismo día.");
        }
    }

    private async Task ValidateExistingAppointmentsAsync(
        ScheduleDay existingDay,
        ScheduleDay? updatedDay)
    {
        var schedule = await _context.ProfessionalSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s =>
                s.Id == existingDay.ProfessionalScheduleId);

        if (schedule is null)
        {
            throw new BusinessException(
                "La agenda asociada no existe.");
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(a =>
                a.ProfessionalId == schedule.ProfessionalId &&
                a.AppointmentDate >= today &&
                a.AppointmentDate >= schedule.StartDate &&
                a.AppointmentDate <= schedule.EndDate &&
                a.Status != AppointmentStatus.Cancelled &&
                a.AppointmentDate.DayOfWeek == existingDay.DayOfWeek &&
                a.StartTime < existingDay.EndTime &&
                a.EndTime > existingDay.StartTime)
            .ToListAsync();

        foreach (var appointment in appointments)
        {
            var remainsCovered = updatedDay is not null &&
                updatedDay.DayOfWeek == appointment.AppointmentDate.DayOfWeek &&
                appointment.StartTime >= updatedDay.StartTime &&
                appointment.EndTime <= updatedDay.EndTime;

            if (!remainsCovered)
            {
                throw new BusinessException(
                    "No se puede modificar o eliminar este bloque porque existen turnos asignados dentro de ese horario.");
            }
        }
    }
}
