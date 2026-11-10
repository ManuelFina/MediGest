
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.ViewModels.Users;

public class UserFormViewModel
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(100, ErrorMessage = "El apellido no puede superar los 100 caracteres.")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
    [StringLength(256, ErrorMessage = "El correo electrónico no puede superar los 256 caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Ingrese un número de teléfono válido.")]
    [StringLength(25, ErrorMessage = "El teléfono no puede superar los 25 caracteres.")]
    [Display(Name = "Teléfono")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un rol.")]
    [Display(Name = "Rol")]
    public string Role { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8,
        ErrorMessage = "La contraseña debe tener entre 8 y 100 caracteres.")]
    [Display(Name = "Contraseña")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Password),
        ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string? ConfirmPassword { get; set; }

    [Display(Name = "Cuenta activa")]
    public bool IsActive { get; set; } = true;

    public IEnumerable<SelectListItem> AvailableRoles { get; set; } =
        new List<SelectListItem>();
}
