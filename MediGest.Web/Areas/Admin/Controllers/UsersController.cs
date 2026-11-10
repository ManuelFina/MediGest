
using MediGest.Web.Models.Identity;
using MediGest.Web.Services.Interfaces;
using MediGest.Web.ViewModels.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MediGest.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = ApplicationRoles.Administrator)]
public class UsersController : Controller
{
    private readonly IUserManagementService _userManagementService;

    public UsersController(
        IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? searchTerm)
    {
        var users = await _userManagementService.SearchAsync(searchTerm);

        var userViewModels = new List<UserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManagementService.GetRolesAsync(user);

            userViewModels.Add(new UserViewModel
            {
                Id = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Role = roles.FirstOrDefault() ?? "Sin rol",
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        var model = new UserListViewModel
        {
            Users = userViewModels,
            SearchTerm = searchTerm,
            TotalUsers = userViewModels.Count,
            ActiveUsers = userViewModels.Count(user => user.IsActive),
            InactiveUsers = userViewModels.Count(user => !user.IsActive)
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new UserFormViewModel();

        await LoadRolesAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(
                nameof(model.Password),
                "La contraseña es obligatoria.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var user = new ApplicationUser
                {
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    Email = model.Email.Trim(),
                    PhoneNumber = model.PhoneNumber?.Trim()
                };

                await _userManagementService.CreateAsync(
                    user,
                    model.Password!,
                    model.Role);

                TempData["Success"] =
                    "El usuario fue creado correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
        }

        await LoadRolesAsync(model);

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManagementService.GetByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        var roles = await _userManagementService.GetRolesAsync(user);

        if (roles.Contains(ApplicationRoles.Administrator))
        {
            return Forbid();
        }

        var model = new UserFormViewModel
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Role = roles.FirstOrDefault() ?? string.Empty,
            IsActive = user.IsActive
        };

        await LoadRolesAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        string id,
        UserFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await LoadRolesAsync(model);
            return View(model);
        }

        try
        {
            var updatedUser = new ApplicationUser
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                IsActive = model.IsActive
            };

            await _userManagementService.UpdateAsync(
                id,
                updatedUser,
                model.Role,
                model.Password);

            TempData["Success"] =
                "El usuario fue actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        await LoadRolesAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string id)
    {
        try
        {
            await _userManagementService.DeactivateAsync(id);

            TempData["Success"] =
                "El usuario fue desactivado correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(string id)
    {
        try
        {
            await _userManagementService.ActivateAsync(id);

            TempData["Success"] =
                "El usuario fue activado correctamente.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private Task LoadRolesAsync(UserFormViewModel model)
    {
        model.AvailableRoles = new[]
        {
            new SelectListItem
            {
                Value = ApplicationRoles.Receptionist,
                Text = "Recepcionista"
            },
            new SelectListItem
            {
                Value = ApplicationRoles.Doctor,
                Text = "Profesional"
            }
        };

        return Task.CompletedTask;
    }
}
