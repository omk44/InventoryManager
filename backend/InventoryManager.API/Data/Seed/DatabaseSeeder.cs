using InventoryManager.API.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InventoryManager.API.Data.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeder");

        // 1. Ensure Roles exist
        string[] roles = ["Admin", "User"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                logger.LogInformation("Role '{Role}' seeded successfully.", role);
            }
        }

        // 2. Ensure Admin User exists
        const string adminEmail = "admin@inventory.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin@Passw0rd2024!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Default Admin user '{Email}' created successfully.", adminEmail);
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // 3. Ensure Default Standard User exists for testing
        const string standardUserEmail = "user@inventory.com";
        var regularUser = await userManager.FindByEmailAsync(standardUserEmail);
        if (regularUser == null)
        {
            regularUser = new ApplicationUser
            {
                UserName = standardUserEmail,
                Email = standardUserEmail,
                FullName = "Standard User",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(regularUser, "User@Passw0rd2024!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(regularUser, "User");
                logger.LogInformation("Default Standard user '{Email}' created successfully.", standardUserEmail);
            }
        }

        // 4. Seed Default Categories if none exist
        if (!await context.Categories.AnyAsync())
        {
            var initialCategories = new List<Category>
            {
                new() { Name = "Electronics", CreatedAt = DateTime.UtcNow, CreatedBy = "SystemSeeder" },
                new() { Name = "Office Supplies", CreatedAt = DateTime.UtcNow, CreatedBy = "SystemSeeder" },
                new() { Name = "Furniture", CreatedAt = DateTime.UtcNow, CreatedBy = "SystemSeeder" },
                new() { Name = "Hardware", CreatedAt = DateTime.UtcNow, CreatedBy = "SystemSeeder" }
            };

            await context.Categories.AddRangeAsync(initialCategories);
            await context.SaveChangesAsync();
            logger.LogInformation("Default categories seeded successfully.");
        }
    }
}
