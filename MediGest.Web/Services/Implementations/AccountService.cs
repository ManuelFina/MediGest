
using MediGest.Web.Models.Identity;
using MediGest.Web.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace MediGest.Web.Services.Implementations;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<ApplicationUser?> FindActiveUserAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);

        return user is { IsActive: true } ? user : null;
    }

    public async Task<SignInResult> SignInAsync(
        ApplicationUser user,
        string password,
        bool rememberMe)
    {
        return await _signInManager.PasswordSignInAsync(
            user,
            password,
            rememberMe,
            lockoutOnFailure: true);
    }

    public async Task SignOutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
