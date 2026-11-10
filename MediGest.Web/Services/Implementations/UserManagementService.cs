
using MediGest.Web.Models.Identity;
using MediGest.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MediGest.Web.Services.Implementations;

public class UserManagementService : IUserManagementService
{
    private readonly UserManager<ApplicationUser> _userManager;

    private static readonly string[] AllowedRoles =
    [
        ApplicationRoles.Receptionist,
        ApplicationRoles.Doctor
    ];

    public UserManagementService(
        UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllAsync()
    {
        return await _userManager.Users
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<ApplicationUser>> SearchAsync(
        string? searchTerm)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();

            query = query.Where(user =>
                user.FirstName.Contains(term) ||
                user.LastName.Contains(term) ||
                user.Email!.Contains(term));
        }

        return await query
            .OrderBy(user => user.LastName)
            .ThenBy(user => user.FirstName)
            .ToListAsync();
    }

    public async Task<ApplicationUser?> GetByIdAsync(string id)
    {
        return await _userManager.FindByIdAsync(id);
    }

    public async Task<IList<string>> GetRolesAsync(
        ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<ApplicationUser> CreateAsync(
        ApplicationUser user,
        string password,
        string role)
    {
        ValidateRole(role);

        user.UserName = user.Email;
        user.EmailConfirmed = true;
        user.IsActive = true;
        user.CreatedAt = DateTime.UtcNow;

        var createResult = await _userManager.CreateAsync(
            user,
            password);

        EnsureSuccess(createResult);

        var roleResult = await _userManager.AddToRoleAsync(
            user,
            role);

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            EnsureSuccess(roleResult);
        }

        return user;
    }

    public async Task UpdateAsync(
        string id,
        ApplicationUser updatedUser,
        string role,
        string? newPassword)
    {
        ValidateRole(role);

        var user = await _userManager.FindByIdAsync(id)
            ?? throw new InvalidOperationException(
                "El usuario solicitado no existe.");

        await EnsureNotAdministratorAsync(user);

        user.FirstName = updatedUser.FirstName.Trim();
        user.LastName = updatedUser.LastName.Trim();
        user.PhoneNumber = updatedUser.PhoneNumber?.Trim();

        var email = updatedUser.Email.Trim();

        var emailResult = await _userManager.SetEmailAsync(
            user,
            email);

        EnsureSuccess(emailResult);

        var usernameResult = await _userManager.SetUserNameAsync(
            user,
            email);

        EnsureSuccess(usernameResult);

        user.IsActive = updatedUser.IsActive;

        var updateResult = await _userManager.UpdateAsync(user);

        EnsureSuccess(updateResult);

        var currentRoles = await _userManager.GetRolesAsync(user);

        var rolesToRemove = currentRoles
            .Where(currentRole => currentRole != role)
            .ToList();

        if (rolesToRemove.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(
                user,
                rolesToRemove);

            EnsureSuccess(removeResult);
        }

        if (!currentRoles.Contains(role))
        {
            var addResult = await _userManager.AddToRoleAsync(
                user,
                role);

            EnsureSuccess(addResult);
        }

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(
                user);

            var passwordResult = await _userManager.ResetPasswordAsync(
                user,
                token,
                newPassword);

            EnsureSuccess(passwordResult);
        }
    }

    public async Task DeactivateAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new InvalidOperationException(
                "El usuario solicitado no existe.");

        await EnsureNotAdministratorAsync(user);

        user.IsActive = false;

        var result = await _userManager.UpdateAsync(user);

        EnsureSuccess(result);
    }

    public async Task ActivateAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new InvalidOperationException(
                "El usuario solicitado no existe.");

        await EnsureNotAdministratorAsync(user);

        user.IsActive = true;

        var result = await _userManager.UpdateAsync(user);

        EnsureSuccess(result);
    }

    private static void ValidateRole(string role)
    {
        if (!AllowedRoles.Contains(role))
        {
            throw new InvalidOperationException(
                "El rol seleccionado no está permitido.");
        }
    }

    private async Task EnsureNotAdministratorAsync(
        ApplicationUser user)
    {
        if (await _userManager.IsInRoleAsync(
                user,
                ApplicationRoles.Administrator))
        {
            throw new InvalidOperationException(
                "No se puede modificar una cuenta de administrador desde este módulo.");
        }
    }

    private static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            var errors = string.Join(
                "; ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(errors);
        }
    }
}
