
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.ViewModels.Invoices;

public class InvoiceDetailFormViewModel
{
    [Required(ErrorMessage = "La prestación es obligatoria.")]
    [Display(Name = "Prestación")]
    public int MedicalServiceId { get; set; }

    [Display(Name = "Turno asociado")]
    public int? AppointmentId { get; set; }

    [Required(ErrorMessage = "La fecha de prestación es obligatoria.")]
    [Display(Name = "Fecha de prestación")]
    public DateOnly ServiceDate { get; set; } =
        DateOnly.FromDateTime(DateTime.Today);

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, 9999, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    [Display(Name = "Cantidad")]
    public int Quantity { get; set; } = 1;

    [StringLength(100, ErrorMessage = "El número de autorización no puede superar los 100 caracteres.")]
    [Display(Name = "Número de autorización")]
    public string? AuthorizationNumber { get; set; }

    public IEnumerable<SelectListItem> Appointments { get; set; } =
        Enumerable.Empty<SelectListItem>();
}
