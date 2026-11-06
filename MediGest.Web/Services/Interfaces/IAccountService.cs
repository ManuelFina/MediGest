
using MediGest.Web.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace MediGest.Web.Services.Interfaces;

public interface IAccountService
{
    Task<ApplicationUser?> FindActiveUserAsync(string email);

    Task<SignInResult> SignInAsync(
        ApplicationUser user,
        string password,
        bool rememberMe);

    Task SignOutAsync();
}
