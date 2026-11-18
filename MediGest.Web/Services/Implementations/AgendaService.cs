
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.Appointments;

namespace MediGest.Web.Services.Implementations;

public class AgendaService : IAgendaService
{
    private readonly IAppointmentService _appointmentService;
    private readonly IAppointmentAvailabilityService _availabilityService;
    private readonly IProfessionalService _professionalService;

    public AgendaService(
        IAppointmentService appointmentService,
        IAppointmentAvailabilityService availabilityService,
        IProfessionalService professionalService)
    {
        _appointmentService = appointmentService;
        _availabilityService = availabilityService;
        _professionalService = professionalService;
    }

    public async Task<DailyAgendaViewModel> GetReceptionistAgendaAsync(
        DateOnly date,
        int? professionalId = null)
    {
        var professionals = (await _professionalService.GetAllAsync())
            .Where(p => p.IsActive)
            .ToList();

        if (professionalId.HasValue)
        {
            professionals = professionals
                .Where(p => p.Id == professionalId.Value)
                .ToList();
        }

        var professionalLookup = professionals.ToDictionary(p => p.Id);

        var agenda = new DailyAgendaViewModel
        {
            SelectedDate = date,
            SelectedProfessionalId = professionalId,
            CanSelectProfessional = true,

            Professionals = professionals
                .Select(p => new AgendaProfessionalViewModel
                {
                    Id = p.Id,
                    FullName = $"{p.ApplicationUser.FirstName} {p.ApplicationUser.LastName}",
                    SpecialtyName = p.Specialty.Name
                })
                .ToList()
        };

        var appointments = professionalId.HasValue
            ? await _appointmentService.GetByProfessionalAsync(
                professionalId.Value,
                date)
            : await _appointmentService.GetByDateAsync(date);

        agenda.Appointments = appointments
            .Where(a => professionalLookup.ContainsKey(a.ProfessionalId))
            .Select(a =>
            {
                var professional = professionalLookup[a.ProfessionalId];

                return new DailyAppointmentViewModel
                {
                    Id = a.Id,
                    ProfessionalId = a.ProfessionalId,
                    PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    PatientDocument = a.Patient.DocumentNumber,
                    ProfessionalName =
                        $"{professional.ApplicationUser.FirstName} {professional.ApplicationUser.LastName}",
                    SpecialtyName = professional.Specialty.Name,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status.ToString()
                };
            })
            .OrderBy(a => a.StartTime)
            .ToList();

        var availableSlots = new List<AvailableAgendaSlotViewModel>();

        foreach (var professional in professionals)
        {
            var slots = await _availabilityService.GetAvailableTimeSlotsAsync(
                professional.Id,
                date);

            availableSlots.AddRange(slots.Select(slot =>
                new AvailableAgendaSlotViewModel
                {
                    ProfessionalId = professional.Id,
                    ProfessionalName =
                        $"{professional.ApplicationUser.FirstName} {professional.ApplicationUser.LastName}",
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime
                }));
        }

        agenda.AvailableSlots = availableSlots
            .OrderBy(s => s.StartTime)
            .ThenBy(s => s.ProfessionalName)
            .ToList();

        return agenda;
    }

    public async Task<DailyAgendaViewModel> GetDoctorAgendaAsync(
        DateOnly date,
        string applicationUserId)
    {
        var professional = await _professionalService
            .GetByApplicationUserIdAsync(applicationUserId);

        if (professional is null || !professional.IsActive)
        {
            throw new InvalidOperationException(
                "No existe un profesional activo asociado al usuario.");
        }

        var appointments = await _appointmentService.GetByProfessionalAsync(
            professional.Id,
            date);

        var slots = await _availabilityService.GetAvailableTimeSlotsAsync(
            professional.Id,
            date);

        var professionalName =
            $"{professional.ApplicationUser.FirstName} {professional.ApplicationUser.LastName}";

        var agenda = new DailyAgendaViewModel
        {
            SelectedDate = date,
            SelectedProfessionalId = professional.Id,
            CanSelectProfessional = false,

            Professionals =
            [
                new AgendaProfessionalViewModel
                {
                    Id = professional.Id,
                    FullName = professionalName,
                    SpecialtyName = professional.Specialty.Name
                }
            ],

            Appointments = appointments
                .Select(a => new DailyAppointmentViewModel
                {
                    Id = a.Id,
                    ProfessionalId = a.ProfessionalId,
                    PatientName = $"{a.Patient.FirstName} {a.Patient.LastName}",
                    PatientDocument = a.Patient.DocumentNumber,
                    ProfessionalName = professionalName,
                    SpecialtyName = professional.Specialty.Name,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    Status = a.Status.ToString()
                })
                .OrderBy(a => a.StartTime)
                .ToList(),

            AvailableSlots = slots
                .Select(slot => new AvailableAgendaSlotViewModel
                {
                    ProfessionalId = professional.Id,
                    ProfessionalName = professionalName,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime
                })
                .ToList()
        };

        return agenda;
    }
}
