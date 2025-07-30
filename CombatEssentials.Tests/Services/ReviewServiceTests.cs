using CombatEssentials.Application.DTOs.ReviewDtos;
using CombatEssentials.Application.Services;
using CombatEssentials.Domain.Entities;
using CombatEssentials.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CombatEssentials.Tests.Services
{
    public class ReviewServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public ReviewServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ReviewServiceTestsDb")
                .Options;
        }

        private ReviewService GetServiceWithSeededDb(List<Review> reviews = null, List<Product> products = null, List<ApplicationUser> users = null)
        {
            var context = new ApplicationDbContext(_dbOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            
            if (users != null)
            {
                context.Users.AddRange(users);
            }
            if (products != null)
            {
                context.Products.AddRange(products);
            }
            if (reviews != null)
            {
                context.Reviews.AddRange(reviews);
            }
            context.SaveChanges();
            return new ReviewService(context);
        }

        [Fact]
        public async Task GetReviewsForProductAsync_ReturnsPaginatedReviews()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", UserName = "testuser", FirstName = "Yanislav", LastName = "Angelov" };
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
            var reviews = Enumerable.Range(1, 20).Select(i => new Review 
            { 
                Id = i, 
                UserId = "user1", 
                ProductId = 1, 
                Rating = 5, 
                Comment = $"Review {i}",
                User = user,
                Product = product
            }).ToList();
            var service = GetServiceWithSeededDb(reviews, new List<Product> { product }, new List<ApplicationUser> { user });

            // Act
            var result = await service.GetReviewsForProductAsync(1, 1);

            // Assert
            Assert.Equal(15, result.Count()); // pageSize = 15
        }

        [Fact]
        public async Task GetReviewsForProductAsync_ReturnsEmpty_WhenNoReviews()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var result = await service.GetReviewsForProductAsync(1, 1);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddReviewAsync_AddsReviewSuccessfully()
        {
            var user = new ApplicationUser { Id = "user1", UserName = "testuser", FirstName="Yanislav", LastName="Angelov" };
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
            var service = GetServiceWithSeededDb(null, new List<Product> { product }, new List<ApplicationUser> { user });
            var dto = new ReviewDto { ProductId = 1, Rating = 5, Comment = "Great product!" };

            // Act
            await service.AddReviewAsync("user1", dto);

            // Assert
            var context = new ApplicationDbContext(_dbOptions);
            var addedReview = await context.Reviews.FirstOrDefaultAsync(r => r.UserId == "user1" && r.ProductId == 1);
            Assert.NotNull(addedReview);
            Assert.Equal(5, addedReview.Rating);
            Assert.Equal("Great product!", addedReview.Comment);
        }

        [Fact]
        public async Task DeleteReviewAsync_DeletesReviewSuccessfully()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", UserName = "testuser", FirstName = "Yanislav", LastName = "Angelov" };
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
            var review = new Review { Id = 1, UserId = "user1", ProductId = 1, Rating = 5, Comment = "Test review" };
            var service = GetServiceWithSeededDb(new List<Review> { review }, new List<Product> { product }, new List<ApplicationUser> { user });

            // Act
            var (success, message) = await service.DeleteReviewAsync("user1", 1);

            // Assert
            Assert.True(success);
            Assert.Equal("Review deleted successfully.", message);
            
            var context = new ApplicationDbContext(_dbOptions);
            var deletedReview = await context.Reviews.FindAsync(1);
            Assert.Null(deletedReview);
        }

        [Fact]
        public async Task DeleteReviewAsync_ReturnsFailure_WhenReviewNotFound()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var (success, message) = await service.DeleteReviewAsync("user1", 999);

            // Assert
            Assert.False(success);
            Assert.Equal("Review not found.", message);
        }

        [Fact]
        public async Task DeleteReviewAsync_ReturnsFailure_WhenUserNotAuthorized()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", UserName = "testuser", FirstName = "Yanislav", LastName = "Angelov" };
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
            var review = new Review { Id = 1, UserId = "user1", ProductId = 1, Rating = 5, Comment = "Test review" };
            var service = GetServiceWithSeededDb(new List<Review> { review }, new List<Product> { product }, new List<ApplicationUser> { user });

            // Act
            var (success, message) = await service.DeleteReviewAsync("user2", 1);

            // Assert
            Assert.False(success);
            Assert.Equal("You are not authorized to delete this review.", message);
        }

        [Fact]
        public async Task GetAverageRatingForProductAsync_ReturnsCorrectAverage()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", UserName = "testuser", FirstName = "Yanislav", LastName = "Angelov" };
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
            var reviews = new List<Review>
            {
                new Review { Id = 1, UserId = "user1", ProductId = 1, Rating = 5, User = user, Product = product, Comment="Sussy" },
                new Review { Id = 2, UserId = "user1", ProductId = 1, Rating = 3, User = user, Product = product, Comment="BAKA"  },
                new Review { Id = 3, UserId = "user1", ProductId = 1, Rating = 4, User = user, Product = product, Comment="Impostor"  }
            };
            var service = GetServiceWithSeededDb(reviews, new List<Product> { product }, new List<ApplicationUser> { user });

            // Act
            var average = await service.GetAverageRatingForProductAsync(1);

            // Assert
            Assert.Equal(4.0, average); // (5 + 3 + 4) / 3 = 4.0
        }

        [Fact]
        public async Task GetAverageRatingForProductAsync_ReturnsZero_WhenNoReviews()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var average = await service.GetAverageRatingForProductAsync(1);

            // Assert
            Assert.Equal(0.0, average);
        }

        [Fact]
        public async Task GetAverageRatingForProductAsync_ReturnsZero_WhenProductHasNoReviews()
        {
            // Arrange
            var user = new ApplicationUser { Id = "user1", UserName = "testuser", FirstName = "Yanislav", LastName = "Angelov" };
            var category = new Category { Id = 1, Name = "Test Category" };
            var product1 = new Product
            {
                Id = 1,
                Name = "P1",
                CategoryId = 1,
                Category = category,
                IsDeleted = false,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var product2 = new Product
            {
                Id = 2,
                Name = "P2",
                CategoryId = 1,
                Category = category,
                IsDeleted = false,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var review = new Review { Id = 1, UserId = "user1", ProductId = 1, Rating = 5, User = user, Product = product1, Comment="BAKA" };
            var service = GetServiceWithSeededDb(new List<Review> { review }, new List<Product> { product1, product2 }, new List<ApplicationUser> { user });

            // Act
            var average = await service.GetAverageRatingForProductAsync(2);

            // Assert
            Assert.Equal(0.0, average);
        }
    }
}
