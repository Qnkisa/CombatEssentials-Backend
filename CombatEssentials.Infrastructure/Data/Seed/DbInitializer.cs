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

        public static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            var users = new List<(string Email, string FirstName, string LastName)>
            {
                ("user1@combat.com", "Ivan", "Ivanov"),
                ("user2@combat.com", "Petar", "Petrov"),
                ("user3@combat.com", "Maria", "Marinova"),
                ("user4@combat.com", "Georgi", "Georgiev"),
                ("user5@combat.com", "Elena", "Elenova")
            };

            foreach (var (email, firstName, lastName) in users)
            {
                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(user, "User123!");
                    await userManager.AddToRoleAsync(user, "User");
                }
            }
        }

        public static async Task SeedShoppingCartsAsync(ApplicationDbContext context)
        {
            if (!await context.ShoppingCarts.AnyAsync())
            {
                var users = await context.Users.Take(5).ToListAsync();
                var carts = users.Select(u => new ShoppingCart { UserId = u.Id }).ToList();
                context.ShoppingCarts.AddRange(carts);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedCartItemsAsync(ApplicationDbContext context)
        {
            if (!await context.CartItems.AnyAsync())
            {
                var carts = await context.ShoppingCarts.Include(sc => sc.CartItems).ToListAsync();
                var products = await context.Products.Take(5).ToListAsync();
                int quantity = 1;
                var cartItems = new List<CartItem>();
                foreach (var cart in carts)
                {
                    foreach (var product in products)
                    {
                        cartItems.Add(new CartItem
                        {
                            ShoppingCartId = cart.Id,
                            ProductId = product.Id,
                            Quantity = quantity
                        });
                        quantity = quantity % 5 + 1;
                        if (cartItems.Count >= 5) break;
                    }
                    if (cartItems.Count >= 5) break;
                }
                context.CartItems.AddRange(cartItems);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedWishlistsAsync(ApplicationDbContext context)
        {
            if (!await context.Wishlists.AnyAsync())
            {
                var users = await context.Users.Take(5).ToListAsync();
                var products = await context.Products.Take(5).ToListAsync();
                var wishlists = new List<Wishlist>();
                foreach (var user in users)
                {
                    foreach (var product in products)
                    {
                        wishlists.Add(new Wishlist
                        {
                            UserId = user.Id,
                            ProductId = product.Id
                        });
                        if (wishlists.Count >= 5) break;
                    }
                    if (wishlists.Count >= 5) break;
                }
                context.Wishlists.AddRange(wishlists);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedOrdersAsync(ApplicationDbContext context)
        {
            if (!await context.Orders.AnyAsync())
            {
                var users = await context.Users.Take(5).ToListAsync();
                var orders = new List<Order>();
                foreach (var user in users)
                {
                    orders.Add(new Order
                    {
                        UserId = user.Id,
                        OrderDate = DateTime.UtcNow,
                        TotalAmount = 100,
                        OrderStatus = CombatEssentials.Domain.Enums.OrderStatus.Pending,
                        ShippingAddress = "123 Main St",
                        FullName = user.FirstName + " " + user.LastName,
                        PhoneNumber = "1234567890"
                    });
                    if (orders.Count >= 5) break;
                }
                context.Orders.AddRange(orders);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedOrderItemsAsync(ApplicationDbContext context)
        {
            if (!await context.OrderItems.AnyAsync())
            {
                var orders = await context.Orders.Include(o => o.OrderItems).ToListAsync();
                var products = await context.Products.Take(5).ToListAsync();
                var orderItems = new List<OrderItem>();
                foreach (var order in orders)
                {
                    foreach (var product in products)
                    {
                        orderItems.Add(new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = product.Id,
                            Quantity = 1,
                            UnitPrice = product.Price,
                            TotalAmount = product.Price
                        });
                        if (orderItems.Count >= 5) break;
                    }
                    if (orderItems.Count >= 5) break;
                }
                context.OrderItems.AddRange(orderItems);
                await context.SaveChangesAsync();
            }
        }

        public static async Task SeedReviewsAsync(ApplicationDbContext context)
        {
            if (!await context.Reviews.AnyAsync())
            {
                var users = await context.Users.Take(5).ToListAsync();
                var products = await context.Products.Take(5).ToListAsync();
                var reviews = new List<Review>();
                int rating = 5;
                foreach (var user in users)
                {
                    foreach (var product in products)
                    {
                        reviews.Add(new Review
                        {
                            UserId = user.Id,
                            ProductId = product.Id,
                            Rating = rating,
                            Comment = $"Great product {product.Name}!"
                        });
                        rating = rating == 1 ? 5 : rating - 1;
                        if (reviews.Count >= 5) break;
                    }
                    if (reviews.Count >= 5) break;
                }
                context.Reviews.AddRange(reviews);
                await context.SaveChangesAsync();
            }
        }
    }

}
