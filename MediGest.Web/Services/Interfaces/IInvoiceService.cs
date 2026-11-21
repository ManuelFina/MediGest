
using MediGest.Web.Models.Entities;

namespace MediGest.Web.Services.Interfaces;

public interface IInvoiceService
{
    Task<IEnumerable<Invoice>> GetAllAsync();

    Task<IEnumerable<Invoice>> GetByPatientAsync(int patientId);

    Task<Invoice?> GetByIdAsync(int id);

    Task<Invoice> CreateAsync(Invoice invoice);

    Task CancelAsync(int id);
}
