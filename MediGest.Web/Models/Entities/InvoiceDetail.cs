
using System.ComponentModel.DataAnnotations;

namespace MediGest.Web.Models.Entities;

public class InvoiceDetail
{
    public int Id { get; set; }

    [Required]
    public int InvoiceId { get; set; }

    public Invoice Invoice { get; set; } = null!;

    [Required]
    [Display(Name = "Prestación")]
    public int MedicalServiceId { get; set; }

    public MedicalService MedicalService { get; set; } = null!;

    [Display(Name = "Turno asociado")]
    public int? AppointmentId { get; set; }
    [Required]
    [Display(Name = "Fecha de prestación")]
    public DateOnly ServiceDate { get; set; }

    [StringLength(100)]
    [Display(Name = "Número de autorización")]
    public string? AuthorizationNumber { get; set; }

    public Appointment? Appointment { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "Descripción")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(1, 9999, ErrorMessage = "La cantidad debe ser mayor a cero.")]
    [Display(Name = "Cantidad")]
    public int Quantity { get; set; } = 1;

    [Required]
    [Range(0.01, 999999999999.99,
        ErrorMessage = "El precio unitario debe ser mayor a cero.")]
    [Display(Name = "Precio unitario")]
    public decimal UnitPrice { get; set; }

    [Required]
    [Range(0, 999999999999.99)]
    [Display(Name = "Cobertura por unidad")]
    public decimal UnitCoverageAmount { get; set; }

    [Required]
    [Range(0, 999999999999.99)]
    [Display(Name = "Subtotal")]
    public decimal Subtotal { get; set; }

    [Required]
    [Range(0, 999999999999.99)]
    [Display(Name = "Importe cubierto")]
    public decimal CoverageAmount { get; set; }

    [Required]
    [Range(0, 999999999999.99)]
    [Display(Name = "Importe a cargo del paciente")]
    public decimal PatientAmount { get; set; }
}
