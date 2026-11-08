
using System.ComponentModel.DataAnnotations;

namespace MediGest.Web.Models.Entities;

public class ScheduleException
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    public DateOnly StartDate { get; set; }

    [Required(ErrorMessage = "La fecha de finalización es obligatoria.")]
    public DateOnly EndDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    [Required(ErrorMessage = "El motivo de la excepción es obligatorio.")]
    [StringLength(200, ErrorMessage = "El motivo no puede superar los 200 caracteres.")]
    public string Reason { get; set; } = string.Empty;

    // Professional relationship
    [Required]
    public int ProfessionalId { get; set; }

    public Professional Professional { get; set; } = null!;
}
