
using System.ComponentModel.DataAnnotations;
using MediGest.Web.Models.Enums;

namespace MediGest.Web.ViewModels.HealthInsurances;

public class HealthInsuranceFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la cobertura es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    [Display(Name = "Nombre")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razón social es obligatoria.")]
    [StringLength(200, ErrorMessage = "La razón social no puede superar los 200 caracteres.")]
    [Display(Name = "Razón social")]
    public string LegalName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El CUIT es obligatorio.")]
    [RegularExpression(@"^\d{11}$", ErrorMessage = "El CUIT debe contener exactamente 11 dígitos.")]
    [Display(Name = "CUIT")]
    public string CUIT { get; set; } = string.Empty;

    [Range(1, 3, ErrorMessage = "Seleccioná un tipo de cobertura válido.")]
    [Display(Name = "Tipo de cobertura")]
    public HealthInsuranceType EntityType { get; set; }

    [StringLength(30, ErrorMessage = "El número de registro no puede superar los 30 caracteres.")]
    [Display(Name = "Número de registro")]
    public string? RegistrationNumber { get; set; }

    [Phone(ErrorMessage = "Ingresá un teléfono válido.")]
    [StringLength(25)]
    [Display(Name = "Teléfono")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Ingresá un correo electrónico válido.")]
    [StringLength(150)]
    [Display(Name = "Correo electrónico")]
    public string? Email { get; set; }

    [StringLength(200, ErrorMessage = "El domicilio no puede superar los 200 caracteres.")]
    [Display(Name = "Domicilio")]
    public string? Address { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    [Display(Name = "Observaciones")]
    public string? Notes { get; set; }
}
