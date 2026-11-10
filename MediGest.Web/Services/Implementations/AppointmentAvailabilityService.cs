
using MediGest.Web.Data;
using MediGest.Web.Models.Enums;
using MediGest.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services.Implementations;

public class AppointmentAvailabilityService : IAppointmentAvailabilityService
{
    private readonly ApplicationDbContext _context;

    public AppointmentAvailabilityService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TimeOnly>> GetAvailableSlotsAsync(
        int professionalId,
        DateOnly date,
        int? excludedAppointmentId = null)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (date < today)
        {
            return Enumerable.Empty<TimeOnly>();
        }

        var professionalExists = await _context.Professionals
            .AnyAsync(p => p.Id == professionalId && p.IsActive);

        if (!professionalExists)
        {
            return Enumerable.Empty<TimeOnly>();
        }

        var schedules = await _context.ProfessionalSchedules
            .AsNoTracking()
            .Include(s => s.ScheduleDays)
            .Where(s =>
                s.ProfessionalId == professionalId &&
                s.IsActive &&
                s.StartDate <= date &&
                s.EndDate >= date)
            .ToListAsync();

        if (schedules.Count == 0)
        {
            return Enumerable.Empty<TimeOnly>();
        }

        var scheduleDays = schedules
            .SelectMany(s => s.ScheduleDays)
            .Where(d => d.DayOfWeek == date.DayOfWeek)
            .OrderBy(d => d.StartTime)
            .ToList();

        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(a =>
                a.ProfessionalId == professionalId &&
                a.AppointmentDate == date &&
                a.Status != AppointmentStatus.Cancelled &&
                (!excludedAppointmentId.HasValue ||
                 a.Id != excludedAppointmentId.Value))
            .ToListAsync();

        var exceptions = await _context.ScheduleExceptions
            .AsNoTracking()
            .Where(e =>
                e.ProfessionalId == professionalId &&
                e.StartDate <= date &&
                e.EndDate >= date)
            .ToListAsync();

        var availableSlots = new List<TimeOnly>();

        foreach (var scheduleDay in scheduleDays)
        {
            var schedule = schedules.First(
                s => s.Id == scheduleDay.ProfessionalScheduleId);

            var duration = schedule.AppointmentDurationMinutes;

            var currentTime = scheduleDay.StartTime;

            while (currentTime.AddMinutes(duration) <= scheduleDay.EndTime)
            {
                var slotEnd = currentTime.AddMinutes(duration);

                var hasAppointmentConflict = appointments.Any(a =>
                    currentTime < a.EndTime &&
                    slotEnd > a.StartTime);

                var hasException = exceptions.Any(e =>
                    !e.StartTime.HasValue ||
                    (currentTime < e.EndTime!.Value &&
                     slotEnd > e.StartTime.Value));

                if (!hasAppointmentConflict && !hasException)
                {
                    availableSlots.Add(currentTime);
                }

                currentTime = slotEnd;
            }
        }

        // Remove duplicated slots when multiple active schedules overlap.
        return availableSlots
            .Distinct()
            .OrderBy(slot => slot)
            .ToList();
    }
}
