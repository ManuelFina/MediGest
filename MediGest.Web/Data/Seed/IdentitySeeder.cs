
using MediGest.Web.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace MediGest.Web.Data.Seed;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var configuration = scope.ServiceProvider
            .GetRequiredService<IConfiguration>();

        // Create application roles
        string[] roles =
        [
            ApplicationRoles.Administrator,
            ApplicationRoles.Receptionist,
            ApplicationRoles.Doctor
        ];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        result.Errors.Select(error => error.Description));

                    throw new InvalidOperationException(
                        $"Could not create role '{role}': {errors}");
                }
            }
        }

        // Read initial administrator credentials from environment variables
        var email = Environment.GetEnvironmentVariable("MEDIGEST_ADMIN_EMAIL");
        var password = Environment.GetEnvironmentVariable("MEDIGEST_ADMIN_PASSWORD");
        var firstName = Environment.GetEnvironmentVariable("MEDIGEST_ADMIN_FIRSTNAME");
        var lastName = Environment.GetEnvironmentVariable("MEDIGEST_ADMIN_LASTNAME");

        // Do not create an administrator if credentials are not configured
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName))
        {
            return;
        }

        var administrator = await userManager.FindByEmailAsync(email);

        if (administrator is not null)
        {
            Console.WriteLine(
                $"Admin has Administrator role: {await userManager.IsInRoleAsync(administrator, ApplicationRoles.Administrator)}");
        }

        if (administrator is null)
        {
            administrator = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(
                administrator,
                password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Could not create initial administrator: {errors}");
            }
        }

        if (!await userManager.IsInRoleAsync(
                administrator,
                ApplicationRoles.Administrator))
        {
            var roleResult = await userManager.AddToRoleAsync(
                administrator,
                ApplicationRoles.Administrator);

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(error => error.Description));

                throw new InvalidOperationException(
                    $"Could not assign Administrator role: {errors}");
            }
        }
    }
}
