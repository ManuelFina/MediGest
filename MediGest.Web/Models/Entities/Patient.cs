using System.ComponentModel.DataAnnotations;

namespace MediGest.Web.Models.Entities;

public class Patient
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(80, ErrorMessage = "El nombre no puede superar los 80 caracteres.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(80, ErrorMessage = "El apellido no puede superar los 80 caracteres.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El DNI es obligatorio.")]
    [StringLength(15, ErrorMessage = "El DNI no puede superar los 15 caracteres.")]
    public string DocumentNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateOnly DateOfBirth { get; set; }

    [Phone(ErrorMessage = "Ingresá un teléfono válido.")]
    [StringLength(25)]
    public string? PhoneNumber { get; set; }

    [EmailAddress(ErrorMessage = "Ingresá un correo electrónico válido.")]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(200, ErrorMessage = "El domicilio no puede superar los 200 caracteres.")]
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Appointment> Appointments { get; set; }
        = new List<Appointment>();
}
