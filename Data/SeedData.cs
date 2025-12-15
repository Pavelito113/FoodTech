using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FoodTech.Data;
using FoodTech.Models;

namespace FoodTech.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;
            
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>(); // ← ИЗМЕНИТЬ НА ApplicationUser
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // Ждем готовности базы данных
            await context.Database.MigrateAsync();

            // Создаем роли
            await SeedRoles(roleManager);
            
            // Создаем администратора
            await SeedAdminUser(userManager);
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Manager", "User", "Guest" };

            foreach (var roleName in roles)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                    Console.WriteLine($"✅ Роль '{roleName}' создана");
                }
            }
        }

        private static async Task SeedAdminUser(UserManager<ApplicationUser> userManager) // ← ИЗМЕНИТЬ НА ApplicationUser
        {
            var adminEmail = "admin@foodtech.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var user = new ApplicationUser // ← ИЗМЕНИТЬ НА ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Администратор",
                    LastName = "Системы",
                    Company = "FoodTech"
                };

                var createResult = await userManager.CreateAsync(user, "Admin123!");

                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "Admin");
                    await userManager.AddToRoleAsync(user, "Manager");
                    Console.WriteLine($"✅ Администратор {adminEmail} создан");
                }
                else
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new Exception($"Ошибка создания администратора: {errors}");
                }
            }
        }
    }
}