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

        public static async Task SeedProductsAsync(ApplicationDbContext context)
        {
            if (!await context.Products.AnyAsync())
            {
                var glovesCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Gloves");
                var headgearCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Headgear");
                var apparelCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Apparel");

                var products = new List<Product>
        {
            new Product
            {
                Name = "Pro Fight Gloves",
                Description = "Durable leather gloves for professional sparring.",
                ImageUrl = "/images/products/gloves1.jpg",
                Price = 89.99m,
                CategoryId = glovesCategory.Id
            },
            new Product
            {
                Name = "Headgear Elite",
                Description = "Maximum protection headgear with breathable padding.",
                ImageUrl = "/images/products/headgear1.jpg",
                Price = 74.50m,
                CategoryId = headgearCategory.Id
            },
            new Product
            {
                Name = "Combat Essentials Hoodie",
                Description = "Comfortable hoodie for everyday wear and training.",
                ImageUrl = "/images/products/apparel1.jpg",
                Price = 49.90m,
                CategoryId = apparelCategory.Id
            }
        };

                context.Products.AddRange(products);
                await context.SaveChangesAsync();
            }
        }
    }

}
