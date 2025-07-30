using CombatEssentials.Application.DTOs.CartDtos;
using CombatEssentials.Application.Services;
using CombatEssentials.Domain.Entities;
using CombatEssentials.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CombatEssentials.Tests.Services
{
    public class CartServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public CartServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CartServiceTestsDb")
                .Options;
        }

        private CartService GetServiceWithSeededDb(List<ShoppingCart> carts = null, List<Product> products = null)
        {
            var context = new ApplicationDbContext(_dbOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            
            if (products != null)
            {
                context.Products.AddRange(products);
            }
            if (carts != null)
            {
                context.ShoppingCarts.AddRange(carts);
            }
            context.SaveChanges();
            return new CartService(context);
        }

        [Fact]
        public async Task GetCartItemsAsync_ReturnsCartItems_WhenCartExists()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 10.0m,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var cart = new ShoppingCart 
            { 
                Id = 1, 
                UserId = "user1", 
                CartItems = new List<CartItem> 
                { 
                    new CartItem { Id = 1, ProductId = 1, Quantity = 2, Product = product } 
                } 
            };
            var service = GetServiceWithSeededDb(new List<ShoppingCart> { cart }, new List<Product> { product });

            // Act
            var result = await service.GetCartItemsAsync("user1");

            // Assert
            Assert.Single(result);
            var item = result.First();
            Assert.Equal(1, item.Id);
            Assert.Equal(1, item.ProductId);
            Assert.Equal("Test Product", item.ProductName);
            Assert.Equal(10.0m, item.ProductPrice);
            Assert.Equal(2, item.Quantity);
            Assert.Equal(20.0m, item.TotalPrice); // 10.0 * 2
        }

        [Fact]
        public async Task GetCartItemsAsync_ReturnsEmpty_WhenCartDoesNotExist()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var result = await service.GetCartItemsAsync("user1");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCartItemsAsync_ReturnsEmpty_WhenCartIsEmpty()
        {
            // Arrange
            var cart = new ShoppingCart { Id = 1, UserId = "user1", CartItems = new List<CartItem>() };
            var service = GetServiceWithSeededDb(new List<ShoppingCart> { cart });

            // Act
            var result = await service.GetCartItemsAsync("user1");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddToCartAsync_AddsItemSuccessfully_WhenCartDoesNotExist()
        {
            // Arrange
            var product = new Product {
                Id = 1,
                Name = "Test Product",
                Price = 10.0m,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var service = GetServiceWithSeededDb(null, new List<Product> { product });
            var dto = new CartItemDto { ProductId = 1, Quantity = 2 };

            // Act
            var (success, message) = await service.AddToCartAsync("user1", dto);

            // Assert
            Assert.True(success);
            Assert.Equal("Item added to cart.", message);
            
            var context = new ApplicationDbContext(_dbOptions);
            var cart = await context.ShoppingCarts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == "user1");
            Assert.NotNull(cart);
            Assert.Single(cart.CartItems);
            Assert.Equal(1, cart.CartItems.First().ProductId);
            Assert.Equal(2, cart.CartItems.First().Quantity);
        }

        [Fact]
        public async Task AddToCartAsync_AddsItemSuccessfully_WhenCartExists()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 10.0m,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var cart = new ShoppingCart { Id = 1, UserId = "user1", CartItems = new List<CartItem>() };
            var service = GetServiceWithSeededDb(new List<ShoppingCart> { cart }, new List<Product> { product });
            var dto = new CartItemDto { ProductId = 1, Quantity = 2 };

            // Act
            var (success, message) = await service.AddToCartAsync("user1", dto);

            // Assert
            Assert.True(success);
            Assert.Equal("Item added to cart.", message);
        }

        [Fact]
        public async Task AddToCartAsync_ReturnsFailure_WhenProductNotFound()
        {
            // Arrange
            var service = GetServiceWithSeededDb();
            var dto = new CartItemDto { ProductId = 999, Quantity = 1 };

            // Act
            var (success, message) = await service.AddToCartAsync("user1", dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Product not found.", message);
        }

        [Fact]
        public async Task AddToCartAsync_ReturnsFailure_WhenQuantityLessThanOne()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 10.0m,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var service = GetServiceWithSeededDb(null, new List<Product> { product });
            var dto = new CartItemDto { ProductId = 1, Quantity = 0 };

            // Act
            var (success, message) = await service.AddToCartAsync("user1", dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Quantity must be at least 1.", message);
        }

        [Fact]
        public async Task AddToCartAsync_ReturnsFailure_WhenItemAlreadyInCart()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 10.0m,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var cart = new ShoppingCart 
            { 
                Id = 1, 
                UserId = "user1", 
                CartItems = new List<CartItem> 
                { 
                    new CartItem { Id = 1, ProductId = 1, Quantity = 1 } 
                } 
            };
            var service = GetServiceWithSeededDb(new List<ShoppingCart> { cart }, new List<Product> { product });
            var dto = new CartItemDto { ProductId = 1, Quantity = 2 };

            // Act
            var (success, message) = await service.AddToCartAsync("user1", dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Item is already in the cart.", message);
        }

        [Fact]
        public async Task RemoveFromCartAsync_RemovesItemSuccessfully()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 10.0m,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var cart = new ShoppingCart 
            { 
                Id = 1, 
                UserId = "user1", 
                CartItems = new List<CartItem> 
                { 
                    new CartItem { Id = 1, ProductId = 1, Quantity = 1 } 
                } 
            };
            var service = GetServiceWithSeededDb(new List<ShoppingCart> { cart }, new List<Product> { product });

            // Act
            var (success, message) = await service.RemoveFromCartAsync("user1", 1);

            // Assert
            Assert.True(success);
            Assert.Equal("Item removed from cart.", message);
            
            var context = new ApplicationDbContext(_dbOptions);
            var removedItem = await context.CartItems.FindAsync(1);
            Assert.Null(removedItem);
        }

        [Fact]
        public async Task RemoveFromCartAsync_ReturnsFailure_WhenItemNotFound()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var (success, message) = await service.RemoveFromCartAsync("user1", 999);

            // Assert
            Assert.False(success);
            Assert.Equal("Item not found or doesn't belong to the user.", message);
        }

        [Fact]
        public async Task RemoveFromCartAsync_ReturnsFailure_WhenItemBelongsToDifferentUser()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 10.0m,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var cart = new ShoppingCart 
            { 
                Id = 1, 
                UserId = "user2", 
                CartItems = new List<CartItem> 
                { 
                    new CartItem { Id = 1, ProductId = 1, Quantity = 1 } 
                } 
            };
            var service = GetServiceWithSeededDb(new List<ShoppingCart> { cart }, new List<Product> { product });

            // Act
            var (success, message) = await service.RemoveFromCartAsync("user1", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("Item not found or doesn't belong to the user.", message);
        }

        [Fact]
        public async Task ClearCartAsync_ClearsCartSuccessfully()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 10.0m,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var cart = new ShoppingCart 
            { 
                Id = 1, 
                UserId = "user1", 
                CartItems = new List<CartItem> 
                { 
                    new CartItem { Id = 1, ProductId = 1, Quantity = 1 },
                    new CartItem { Id = 2, ProductId = 1, Quantity = 2 }
                } 
            };
            var service = GetServiceWithSeededDb(new List<ShoppingCart> { cart }, new List<Product> { product });

            // Act
            var (success, message) = await service.ClearCartAsync("user1");

            // Assert
            Assert.True(success);
            Assert.Equal("Cart cleared.", message);
            
            var context = new ApplicationDbContext(_dbOptions);
            var clearedCart = await context.ShoppingCarts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == "user1");
            Assert.Empty(clearedCart.CartItems);
        }

        [Fact]
        public async Task ClearCartAsync_ReturnsFailure_WhenCartDoesNotExist()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var (success, message) = await service.ClearCartAsync("user1");

            // Assert
            Assert.False(success);
            Assert.Equal("Cart is already empty.", message);
        }

        [Fact]
        public async Task ClearCartAsync_ReturnsFailure_WhenCartIsAlreadyEmpty()
        {
            // Arrange
            var cart = new ShoppingCart { Id = 1, UserId = "user1", CartItems = new List<CartItem>() };
            var service = GetServiceWithSeededDb(new List<ShoppingCart> { cart });

            // Act
            var (success, message) = await service.ClearCartAsync("user1");

            // Assert
            Assert.False(success);
            Assert.Equal("Cart is already empty.", message);
        }

        [Fact]
        public async Task UpdateQuantityAsync_UpdatesQuantitySuccessfully()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 10.0m,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var cart = new ShoppingCart 
            { 
                Id = 1, 
                UserId = "user1", 
                CartItems = new List<CartItem> 
                { 
                    new CartItem { Id = 1, ProductId = 1, Quantity = 1 } 
                } 
            };
            var service = GetServiceWithSeededDb(new List<ShoppingCart> { cart }, new List<Product> { product });

            // Act
            var (success, message) = await service.UpdateQuantityAsync("user1", 1, 3);

            // Assert
            Assert.True(success);
            Assert.Equal("Quantity updated.", message);
            
            var context = new ApplicationDbContext(_dbOptions);
            var updatedItem = await context.CartItems.FindAsync(1);
            Assert.Equal(3, updatedItem.Quantity);
        }

        [Fact]
        public async Task UpdateQuantityAsync_ReturnsFailure_WhenQuantityLessThanOne()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var (success, message) = await service.UpdateQuantityAsync("user1", 1, 0);

            // Assert
            Assert.False(success);
            Assert.Equal("Quantity must be at least 1.", message);
        }

        [Fact]
        public async Task UpdateQuantityAsync_ReturnsFailure_WhenItemNotFound()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var (success, message) = await service.UpdateQuantityAsync("user1", 999, 2);

            // Assert
            Assert.False(success);
            Assert.Equal("Cart item not found or doesn't belong to the user.", message);
        }

        [Fact]
        public async Task UpdateQuantityAsync_ReturnsFailure_WhenItemBelongsToDifferentUser()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 10.0m,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var cart = new ShoppingCart 
            { 
                Id = 1, 
                UserId = "user2", 
                CartItems = new List<CartItem> 
                { 
                    new CartItem { Id = 1, ProductId = 1, Quantity = 1 } 
                } 
            };
            var service = GetServiceWithSeededDb(new List<ShoppingCart> { cart }, new List<Product> { product });

            // Act
            var (success, message) = await service.UpdateQuantityAsync("user1", 1, 2);

            // Assert
            Assert.False(success);
            Assert.Equal("Cart item not found or doesn't belong to the user.", message);
        }
    }
}
