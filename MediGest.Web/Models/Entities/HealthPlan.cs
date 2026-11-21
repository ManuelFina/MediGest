
using System.ComponentModel.DataAnnotations;

namespace MediGest.Web.Models.Entities;

public class HealthPlan
{
    public int Id { get; set; }

    [Required]
    public int HealthInsuranceId { get; set; }

    public HealthInsurance HealthInsurance { get; set; } = null!;

    [Required(ErrorMessage = "El nombre del plan es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(30, ErrorMessage = "El código no puede superar los 30 caracteres.")]
    public string? Code { get; set; }

    [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    public string? Description { get; set; }

    public bool RequiresAuthorization { get; set; }

    public bool IsActive { get; set; } = true;
    public ICollection<PatientAffiliation> Affiliations { get; set; }
    = new List<PatientAffiliation>();
}
