
using MediGest.Web.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MediGest.Web.Models.Entities;

public class Payment
{
    public int Id { get; set; }

    [Required]
    public int InvoiceId { get; set; }

    public Invoice Invoice { get; set; } = null!;

    [Required(ErrorMessage = "La fecha del pago es obligatoria.")]
    [Display(Name = "Fecha de pago")]
    public DateTime PaymentDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "El importe es obligatorio.")]
    [Range(0.01, 999999999999.99,
        ErrorMessage = "El importe debe ser mayor a cero.")]
    [Display(Name = "Importe")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "El medio de pago es obligatorio.")]
    [Display(Name = "Medio de pago")]
    public PaymentMethod Method { get; set; }

    [StringLength(100, ErrorMessage = "La referencia no puede superar los 100 caracteres.")]
    [Display(Name = "Número de operación")]
    public string? Reference { get; set; }

    [StringLength(500, ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    [Display(Name = "Observaciones")]
    public string? Notes { get; set; }
}
