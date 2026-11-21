
using System.ComponentModel.DataAnnotations;
using MediGest.Web.Models.Enums;

namespace MediGest.Web.Models.Entities;

public class HealthInsurance
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la cobertura es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razón social es obligatoria.")]
    [StringLength(200, ErrorMessage = "La razón social no puede superar los 200 caracteres.")]
    public string LegalName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El CUIT es obligatorio.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "El CUIT debe contener exactamente 11 dígitos.")]
    public string CUIT { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de cobertura es obligatorio.")]
    public HealthInsuranceType EntityType { get; set; }

    [StringLength(30, ErrorMessage = "El número de registro no puede superar los 30 caracteres.")]
    public string? RegistrationNumber { get; set; }

    [Phone(ErrorMessage = "Ingresá un teléfono válido.")]
    [StringLength(25)]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Ingresá un correo electrónico válido.")]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(200, ErrorMessage = "El domicilio no puede superar los 200 caracteres.")]
    public string? Address { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<HealthPlan> Plans { get; set; }
        = new List<HealthPlan>();
    public ICollection<PatientAffiliation> Affiliations { get; set; }
    = new List<PatientAffiliation>();
}
