using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using CombatEssentials.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CombatEssentials.Infrastructure.Data.Seed
{

    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed admin user
            var adminEmail = "admin@combat.com";
            var adminFirstName = "Admin";
            var adminLastName = "Adminov";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    FirstName = adminFirstName,
                    LastName = adminLastName,
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!"); // secure in production
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        public static async Task SeedCategoriesAsync(ApplicationDbContext context)
        {
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Gloves" },
                    new Category { Name = "Headgear" },
                    new Category { Name = "Shin Guards" },
                    new Category { Name = "Mouthguards" },
                    new Category { Name = "Training Pads" },
                    new Category { Name = "Apparel" },
                    new Category { Name = "Footwear" },
                    new Category { Name = "Weapons" }
                };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }
        }
    }

}
