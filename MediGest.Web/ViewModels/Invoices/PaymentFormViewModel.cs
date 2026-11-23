
using System.ComponentModel.DataAnnotations;
using MediGest.Web.Models.Enums;

namespace MediGest.Web.ViewModels.Invoices;

public class PaymentFormViewModel
{
    [Required]
    public int InvoiceId { get; set; }

    [Required(ErrorMessage = "La fecha del pago es obligatoria.")]
    [Display(Name = "Fecha de pago")]
    public DateTime PaymentDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "El medio de pago es obligatorio.")]
    [Display(Name = "Medio de pago")]
    public PaymentMethod Method { get; set; } = PaymentMethod.Cash;

    [StringLength(100,
        ErrorMessage = "La referencia no puede superar los 100 caracteres.")]
    [Display(Name = "Número de operación")]
    public string? Reference { get; set; }

    [StringLength(500,
        ErrorMessage = "Las observaciones no pueden superar los 500 caracteres.")]
    [Display(Name = "Observaciones")]
    public string? Notes { get; set; }

    // Información de la factura

    public string InvoiceNumber { get; set; } = string.Empty;

    public string PatientName { get; set; } = string.Empty;

    public decimal InvoiceAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal PendingAmount { get; set; }
}
