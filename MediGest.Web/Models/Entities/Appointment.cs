
using System.ComponentModel.DataAnnotations;
using MediGest.Web.Models.Enums;

namespace MediGest.Web.Models.Entities;

public class Appointment
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El paciente es obligatorio.")]
    public int PatientId { get; set; }

    public Patient Patient { get; set; } = null!;

    [Required(ErrorMessage = "El profesional es obligatorio.")]
    public int ProfessionalId { get; set; }

    public Professional Professional { get; set; } = null!;

    [Required(ErrorMessage = "La fecha del turno es obligatoria.")]
    public DateOnly AppointmentDate { get; set; }

    [Required(ErrorMessage = "La hora de inicio es obligatoria.")]
    public TimeOnly StartTime { get; set; }

    [Required(ErrorMessage = "La hora de finalización es obligatoria.")]
    public TimeOnly EndTime { get; set; }

    public AppointmentStatus Status { get; set; }
        = AppointmentStatus.Pending;

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    // Appointment history relationship
    public ICollection<AppointmentHistory> History { get; set; }
        = new List<AppointmentHistory>();
}
