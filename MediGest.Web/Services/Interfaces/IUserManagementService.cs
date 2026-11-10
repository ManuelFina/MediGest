
using MediGest.Web.Models.Identity;

namespace MediGest.Web.Services.Interfaces;

public interface IUserManagementService
{
    Task<IEnumerable<ApplicationUser>> GetAllAsync();

    Task<IEnumerable<ApplicationUser>> SearchAsync(string? searchTerm);

    Task<ApplicationUser?> GetByIdAsync(string id);

    Task<IList<string>> GetRolesAsync(ApplicationUser user);

    Task<ApplicationUser> CreateAsync(
        ApplicationUser user,
        string password,
        string role);

    Task UpdateAsync(
        string id,
        ApplicationUser updatedUser,
        string role,
        string? newPassword);

    Task DeactivateAsync(string id);

    Task ActivateAsync(string id);
}
