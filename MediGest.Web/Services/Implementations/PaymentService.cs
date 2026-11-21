
using MediGest.Web.Data;
using MediGest.Web.Exceptions;
using MediGest.Web.Models.Entities;
using MediGest.Web.Models.Enums;
using MediGest.Web.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Payment>> GetByInvoiceAsync(int invoiceId)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<Payment> RegisterAsync(Payment payment)
    {
        if (payment is null)
        {
            throw new BusinessException(
                "Los datos del pago son obligatorios.");
        }

        if (payment.Amount <= 0)
        {
            throw new BusinessException(
                "El importe del pago debe ser mayor a cero.");
        }

        if (!Enum.IsDefined(typeof(PaymentMethod), payment.Method))
        {
            throw new BusinessException(
                "El medio de pago seleccionado no es válido.");
        }

        if (payment.PaymentDate == default)
        {
            throw new BusinessException(
                "La fecha del pago es obligatoria.");
        }

        if (payment.PaymentDate.Date > DateTime.Today)
        {
            throw new BusinessException(
                "La fecha del pago no puede ser futura.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        var invoice = await _context.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == payment.InvoiceId);

        if (invoice is null)
        {
            throw new BusinessException("La factura no existe.");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            throw new BusinessException(
                "No se pueden registrar pagos sobre una factura anulada.");
        }

        var paidAmount = invoice.Payments.Sum(p => p.Amount);
        var pendingAmount = invoice.PatientAmount - paidAmount;

        if (pendingAmount <= 0)
        {
            throw new BusinessException(
                "La factura ya se encuentra completamente pagada.");
        }

        if (payment.Amount > pendingAmount)
        {
            throw new BusinessException(
                $"El importe supera el saldo pendiente de ${pendingAmount:N2}.");
        }

        var newPayment = new Payment
        {
            InvoiceId = invoice.Id,
            PaymentDate = payment.PaymentDate,
            Amount = Math.Round(payment.Amount, 2),
            Method = payment.Method,
            Reference = string.IsNullOrWhiteSpace(payment.Reference)
                ? null
                : payment.Reference.Trim(),
            Notes = string.IsNullOrWhiteSpace(payment.Notes)
                ? null
                : payment.Notes.Trim()
        };

        _context.Payments.Add(newPayment);

        var newPaidAmount = paidAmount + newPayment.Amount;

        invoice.Status = newPaidAmount >= invoice.PatientAmount
            ? InvoiceStatus.Paid
            : InvoiceStatus.PartiallyPaid;

        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return newPayment;
    }
}
