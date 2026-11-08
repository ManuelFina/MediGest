
using System.ComponentModel.DataAnnotations;

namespace MediGest.Web.ViewModels.Patients;

public class PatientFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres.")]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(80, ErrorMessage = "El apellido no puede superar los 80 caracteres.")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El documento es obligatorio.")]
    [StringLength(15, ErrorMessage = "El documento no puede superar los 15 caracteres.")]
    [Display(Name = "Documento")]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de nacimiento")]
    public DateOnly DateOfBirth { get; set; }

    [Phone(ErrorMessage = "Ingresá un teléfono válido.")]
    [StringLength(25, ErrorMessage = "El teléfono no puede superar los 25 caracteres.")]
    [Display(Name = "Teléfono")]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Ingresá un correo electrónico válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar los 150 caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string? Email { get; set; }

    [StringLength(200, ErrorMessage = "El domicilio no puede superar los 200 caracteres.")]
    [Display(Name = "Domicilio")]
    public string? Address { get; set; }
}
