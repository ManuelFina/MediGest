
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
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
                "No se encontró el horario que intentás modificar.");
        }

        await ValidateScheduleDayAsync(scheduleDay, scheduleDay.Id);

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
                "No se encontró el horario que intentás eliminar.");
        }

        _context.ScheduleDays.Remove(scheduleDay);

        await _context.SaveChangesAsync();
    }

    private async Task ValidateScheduleDayAsync(
        ScheduleDay scheduleDay,
        int? excludedId = null)
    {
        if (scheduleDay.StartTime >= scheduleDay.EndTime)
        {
            throw new BusinessException(
                "La hora de finalización debe ser posterior a la hora de inicio.");
        }

        var scheduleExists = await _context.ProfessionalSchedules
            .AnyAsync(s => s.Id == scheduleDay.ProfessionalScheduleId);

        if (!scheduleExists)
        {
            throw new BusinessException(
                "La agenda seleccionada no existe.");
        }

        var hasOverlappingTime = await _context.ScheduleDays
            .AnyAsync(d =>
                d.ProfessionalScheduleId == scheduleDay.ProfessionalScheduleId &&
                d.DayOfWeek == scheduleDay.DayOfWeek &&
                d.Id != excludedId &&
                scheduleDay.StartTime < d.EndTime &&
                scheduleDay.EndTime > d.StartTime);

        if (hasOverlappingTime)
        {
            throw new BusinessException(
                "El horario se superpone con otra franja configurada para ese día.");
        }
    }
}
