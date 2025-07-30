using CombatEssentials.Application.DTOs.WishlistDtos;
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
    public class WishlistServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public WishlistServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "WishlistServiceTestsDb")
                .Options;
        }

        private WishlistService GetServiceWithSeededDb(List<Wishlist> wishlistItems = null, List<Product> products = null, List<Category> categories = null)
        {
            var context = new ApplicationDbContext(_dbOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            
            if (categories != null)
            {
                context.Categories.AddRange(categories);
            }
            if (products != null)
            {
                context.Products.AddRange(products);
            }
            if (wishlistItems != null)
            {
                context.Wishlists.AddRange(wishlistItems);
            }
            context.SaveChanges();
            return new WishlistService(context);
        }

        [Fact]
        public async Task GetWishlistAsync_ReturnsPaginatedWishlist()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test Category" };
            var products = Enumerable.Range(1, 20).Select(i => new Product 
            { 
                Id = i, 
                Name = $"Product {i}", 
                Price = 10.0m, 
                Description = $"Description {i}",
                ImageUrl = $"/images/{i}.jpg",
                Category = category,
                CategoryId = 1
            }).ToList();
            var wishlistItems = Enumerable.Range(1, 20).Select(i => new Wishlist 
            { 
                Id = i, 
                UserId = "user1", 
                ProductId = i 
            }).ToList();
            var service = GetServiceWithSeededDb(wishlistItems, products, new List<Category> { category });

            // Act
            var result = await service.GetWishlistAsync("user1", 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20, result.TotalCount);
            Assert.Equal(15, result.Products.Count()); // pageSize = 15
        }

        [Fact]
        public async Task GetWishlistAsync_ReturnsEmpty_WhenNoWishlistItems()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var result = await service.GetWishlistAsync("user1", 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalCount);
            Assert.Empty(result.Products);
        }

        [Fact]
        public async Task GetWishlistAsync_ReturnsOnlyUserWishlist()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test Category" };
            var product = new Product
            {
                Id = 1,
                Name = "P1",
                CategoryId = 1,
                Category = category,
                IsDeleted = false,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var wishlistItems = new List<Wishlist>
            {
                new Wishlist { Id = 1, UserId = "user1", ProductId = 1 },
                new Wishlist { Id = 2, UserId = "user2", ProductId = 1 }
            };
            var service = GetServiceWithSeededDb(wishlistItems, new List<Product> { product }, new List<Category> { category });

            // Act
            var result = await service.GetWishlistAsync("user1", 1);

            // Assert
            Assert.Equal(1, result.TotalCount);
            Assert.Single(result.Products);
        }

        [Fact]
        public async Task AddToWishlistAsync_AddsProductSuccessfully()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test Category" };
            var product = new Product
            {
                Id = 1,
                Name = "P1",
                CategoryId = 1,
                Category = category,
                IsDeleted = false,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var service = GetServiceWithSeededDb(null, new List<Product> { product }, new List<Category> { category });

            // Act
            var (success, message) = await service.AddToWishlistAsync("user1", 1);

            // Assert
            Assert.True(success);
            Assert.Equal("Product added to wishlist.", message);
            
            var context = new ApplicationDbContext(_dbOptions);
            var addedItem = await context.Wishlists.FirstOrDefaultAsync(w => w.UserId == "user1" && w.ProductId == 1);
            Assert.NotNull(addedItem);
        }

        [Fact]
        public async Task AddToWishlistAsync_ReturnsFailure_WhenUserNotAuthenticated()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var (success, message) = await service.AddToWishlistAsync("", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("User is not authenticated.", message);
        }

        [Fact]
        public async Task AddToWishlistAsync_ReturnsFailure_WhenProductNotFound()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var (success, message) = await service.AddToWishlistAsync("user1", 999);

            // Assert
            Assert.False(success);
            Assert.Equal("Product not found.", message);
        }

        [Fact]
        public async Task AddToWishlistAsync_ReturnsFailure_WhenProductAlreadyInWishlist()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test Category" };
            var product = new Product
            {
                Id = 1,
                Name = "P1",
                CategoryId = 1,
                Category = category,
                IsDeleted = false,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var wishlistItem = new Wishlist { Id = 1, UserId = "user1", ProductId = 1 };
            var service = GetServiceWithSeededDb(new List<Wishlist> { wishlistItem }, new List<Product> { product }, new List<Category> { category });

            // Act
            var (success, message) = await service.AddToWishlistAsync("user1", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("Product is already in the wishlist.", message);
        }

        [Fact]
        public async Task RemoveFromWishlistAsync_RemovesProductSuccessfully()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test Category" };
            var product = new Product
            {
                Id = 1,
                Name = "P1",
                CategoryId = 1,
                Category = category,
                IsDeleted = false,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var wishlistItem = new Wishlist { Id = 1, UserId = "user1", ProductId = 1 };
            var service = GetServiceWithSeededDb(new List<Wishlist> { wishlistItem }, new List<Product> { product }, new List<Category> { category });

            // Act
            var (success, message) = await service.RemoveFromWishlistAsync("user1", 1);

            // Assert
            Assert.True(success);
            Assert.Equal("Product removed from wishlist.", message);
            
            var context = new ApplicationDbContext(_dbOptions);
            var removedItem = await context.Wishlists.FirstOrDefaultAsync(w => w.UserId == "user1" && w.ProductId == 1);
            Assert.Null(removedItem);
        }

        [Fact]
        public async Task RemoveFromWishlistAsync_ReturnsFailure_WhenUserNotAuthenticated()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var (success, message) = await service.RemoveFromWishlistAsync("", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("User is not authenticated.", message);
        }

        [Fact]
        public async Task RemoveFromWishlistAsync_ReturnsFailure_WhenProductNotInWishlist()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test Category" };
            var product = new Product
            {
                Id = 1,
                Name = "P1",
                CategoryId = 1,
                Category = category,
                IsDeleted = false,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var service = GetServiceWithSeededDb(null, new List<Product> { product }, new List<Category> { category });

            // Act
            var (success, message) = await service.RemoveFromWishlistAsync("user1", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("Product not found in wishlist.", message);
        }

        [Fact]
        public async Task RemoveFromWishlistAsync_ReturnsFailure_WhenProductNotInUserWishlist()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test Category" };
            var product = new Product
            {
                Id = 1,
                Name = "P1",
                CategoryId = 1,
                Category = category,
                IsDeleted = false,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var wishlistItem = new Wishlist { Id = 1, UserId = "user2", ProductId = 1 };
            var service = GetServiceWithSeededDb(new List<Wishlist> { wishlistItem }, new List<Product> { product }, new List<Category> { category });

            // Act
            var (success, message) = await service.RemoveFromWishlistAsync("user1", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("Product not found in wishlist.", message);
        }
    }
}
