
using System.ComponentModel.DataAnnotations;
using MediGest.Web.Models.Identity;

namespace MediGest.Web.Models.Entities;

public class AppointmentHistory
{
    public int Id { get; set; }

    [Required]
    public int AppointmentId { get; set; }

    public Appointment Appointment { get; set; } = null!;

    [Required(ErrorMessage = "La descripción del cambio es obligatoria.")]
    [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    public string Description { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string ChangedByUserId { get; set; } = string.Empty;

    public ApplicationUser ChangedByUser { get; set; } = null!;

    public ICollection<AppointmentHistory> History { get; set; }
    = new List<AppointmentHistory>();
}
