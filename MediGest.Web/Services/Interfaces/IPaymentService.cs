
using MediGest.Web.Models.Entities;

namespace MediGest.Web.Services.Interfaces;

public interface IPaymentService
{
    Task<IEnumerable<Payment>> GetByInvoiceAsync(int invoiceId);

    Task<Payment> RegisterAsync(Payment payment);
}
