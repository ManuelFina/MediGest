
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.Account;
using Microsoft.AspNetCore.Mvc;

namespace MediGest.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;

        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);
        
        var user = await _accountService.FindActiveUserAsync(model.Email);

        if (user is null)
        {
            ModelState.AddModelError(
                string.Empty,
                "El correo electrónico o la contraseña son incorrectos.");

            return View(model);
        }

        var result = await _accountService.SignInAsync(
            user,
            model.Password,
            model.RememberMe);

        if (result.Succeeded)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        var errorMessage = result.IsLockedOut
            ? "La cuenta está bloqueada temporalmente. Intentá nuevamente más tarde."
            : "El correo electrónico o la contraseña son incorrectos.";

        ModelState.AddModelError(string.Empty, errorMessage);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.SignOutAsync();

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
