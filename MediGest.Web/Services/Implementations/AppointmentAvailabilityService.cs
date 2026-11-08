
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
        DateOnly date)
    {
        // 1. Obtener la agenda vigente del profesional.
        var schedule = await _context.ProfessionalSchedules
            .AsNoTracking()
            .Include(s => s.ScheduleDays)
            .Where(s =>
                s.ProfessionalId == professionalId &&
                s.IsActive &&
                s.StartDate <= date &&
                s.EndDate >= date)
            .FirstOrDefaultAsync();

        if (schedule is null)
            return Enumerable.Empty<TimeOnly>();

        // 2. Obtener las franjas horarias del día solicitado.
        var daySchedules = schedule.ScheduleDays
            .Where(d => d.DayOfWeek == date.DayOfWeek)
            .OrderBy(d => d.StartTime)
            .ToList();

        if (daySchedules.Count == 0)
            return Enumerable.Empty<TimeOnly>();

        // 3. Obtener los turnos que no están cancelados.
        var appointments = await _context.Appointments
            .AsNoTracking()
            .Where(a =>
                a.ProfessionalId == professionalId &&
                a.AppointmentDate == date &&
                a.Status != AppointmentStatus.Cancelled)
            .ToListAsync();

        // 4. Obtener las excepciones correspondientes a esa fecha.
        var exceptions = await _context.ScheduleExceptions
            .AsNoTracking()
            .Where(e =>
                e.ProfessionalId == professionalId &&
                e.StartDate <= date &&
                e.EndDate >= date)
            .ToListAsync();

        // 5. Generar y filtrar los horarios disponibles.
        var availableSlots = new List<TimeOnly>();

        foreach (var daySchedule in daySchedules)
        {
            var currentTime = daySchedule.StartTime;

            while (currentTime.AddMinutes(
                       schedule.AppointmentDurationMinutes)
                   <= daySchedule.EndTime)
            {
                var slotEnd = currentTime.AddMinutes(
                    schedule.AppointmentDurationMinutes);

                var hasAppointment = appointments.Any(a =>
                    currentTime < a.EndTime &&
                    slotEnd > a.StartTime);

                var hasException = exceptions.Any(e =>
                    !e.StartTime.HasValue ||
                    (currentTime < e.EndTime!.Value &&
                     slotEnd > e.StartTime.Value));

                if (!hasAppointment && !hasException)
                {
                    availableSlots.Add(currentTime);
                }

                currentTime = slotEnd;
            }
        }

        return availableSlots
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }
}
