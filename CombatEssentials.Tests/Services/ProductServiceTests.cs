using CombatEssentials.Application.DTOs.ProductDtos;
using CombatEssentials.Application.Services;
using CombatEssentials.Domain.Entities;
using CombatEssentials.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CombatEssentials.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public ProductServiceTests()
        {
            _envMock = new Mock<IWebHostEnvironment>();
            _envMock.Setup(e => e.WebRootPath).Returns(Directory.GetCurrentDirectory());
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ProductServiceTestsDb")
                .Options;
        }

        private ProductService GetServiceWithSeededDb(List<Product> products = null, List<Category> categories = null)
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
            context.SaveChanges();
            return new ProductService(context, _envMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsPaginatedProducts()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Cat1" };
            var products = Enumerable.Range(1, 20)
                .Select(i => new Product
                {
                    Id = i,
                    Name = $"P{i}",
                    Category = category,
                    CategoryId = 1,
                    Description = "sus",
                    ImageUrl = $"/images/p{i}.jpg"
                })
                .ToList();
            var service = GetServiceWithSeededDb(products, new List<Category> { category });

            // Act
            var result = await service.GetAllAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(15, result.Products.Count()); // pageSize = 15
            Assert.Equal(20, result.TotalItems);
        }

        [Fact]
        public async Task GetAllAsync_FiltersByCategoryAndName()
        {
            // Arrange
            var cat1 = new Category { Id = 1, Name = "Cat1" };
            var cat2 = new Category { Id = 2, Name = "Cat2" };
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Boxing Gloves", Category = cat1, CategoryId = 1, ImageUrl="imposotr", Description="sus" },
                new Product { Id = 2, Name = "MMA Gloves", Category = cat2, CategoryId = 2, ImageUrl="baka", Description="baka"  },
                new Product { Id = 3, Name = "Boxing Headgear", Category = cat1, CategoryId = 1 , ImageUrl="amogus", Description="impostor" }
            };
            var service = GetServiceWithSeededDb(products, new List<Category> { cat1, cat2 });

            // Act
            var byCat = await service.GetAllAsync(1, categoryId: 1);
            var byName = await service.GetAllAsync(1, name: "Boxing");

            // Assert
            Assert.All(byCat.Products, p => Assert.Equal(1, p.CategoryId));
            Assert.All(byName.Products, p => Assert.Contains("Boxing", p.Name));
        }

        [Fact]
        public async Task GetRandomProductsAsync_ReturnsRandomProducts()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Cat1" };
            var products = Enumerable.Range(1, 20).Select(i => new Product { Id = i, Name = "Old", Description = "Old", Price = 1, Category = category, CategoryId = 1, ImageUrl = "sussy" }).ToList();
            var service = GetServiceWithSeededDb(products, new List<Category> { category });

            // Act
            var result = await service.GetRandomProductsAsync(5);

            // Assert
            Assert.Equal(5, result.Count());
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsProduct_WhenExists()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Cat1" };
            var product = new Product {Id = 1, Name = "P1", Category = category, CategoryId = 1, IsDeleted = false, Description = "Test Description", ImageUrl = "/images/test.jpg" };
            var service = GetServiceWithSeededDb(new List<Product> { product }, new List<Category> { category });

            // Act
            var result = await service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_CreatesProductSuccessfully()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Cat1" };
            var service = GetServiceWithSeededDb(null, new List<Category> { category });
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.jpg");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Returns(Task.CompletedTask);
            var dto = new CreateProductDto { Name = "P1", Description = "Desc", Price = 10, CategoryId = 1, ImageFile = fileMock.Object };

            // Act
            var (success, message, created) = await service.CreateAsync(dto);

            // Assert
            Assert.True(success);
            Assert.NotNull(created);
            Assert.Equal("P1", created.Name);
        }

        [Fact]
        public async Task CreateAsync_ReturnsFailure_WhenImageSaveFails()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Cat1" };
            var service = GetServiceWithSeededDb(null, new List<Category> { category });
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.jpg");
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default)).Throws(new IOException());
            var dto = new CreateProductDto { Name = "P1", Description = "Desc", Price = 10, CategoryId = 1, ImageFile = fileMock.Object };

            // Act & Assert
            await Assert.ThrowsAsync<IOException>(() => service.CreateAsync(dto));
        }

        [Fact]
        public async Task UpdateAsync_UpdatesProductSuccessfully()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Cat1" };
            var product = new Product { Id = 1, Name = "Old", Description = "Old", Price = 1, Category = category, CategoryId = 1, ImageUrl = "niki" };
            var service = GetServiceWithSeededDb(new List<Product> { product }, new List<Category> { category });
            var dto = new UpdateProductDto { Name = "New", Description = "New", Price = 2, CategoryId = 1, ImageFile = null };

            // Act
            var (success, message) = await service.UpdateAsync(1, dto);

            // Assert
            Assert.True(success);
            var updated = await service.GetByIdAsync(1);
            Assert.Equal("New", updated.Name);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsFailure_WhenProductNotFound()
        {
            // Arrange
            var service = GetServiceWithSeededDb();
            var dto = new UpdateProductDto { Name = "New", Description = "New", Price = 2, CategoryId = 1, ImageFile = null };

            // Act
            var (success, message) = await service.UpdateAsync(999, dto);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public async Task DeleteAsync_DeletesProductSuccessfully()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Cat1" };
            var product = new Product
            {
                Id = 1,
                Name = "P1",
                Category = category,
                CategoryId = 1,
                IsDeleted = false,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var service = GetServiceWithSeededDb(new List<Product> { product }, new List<Category> { category });

            // Act
            var (success, message) = await service.DeleteAsync(1);

            // Assert
            Assert.True(success);
            var deleted = await service.GetByIdAsync(1);
            // IsDeleted is not exposed in ProductDto, but you can check via context if needed
        }

        [Fact]
        public async Task DeleteAsync_ReturnsFailure_WhenProductNotFound()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var (success, message) = await service.DeleteAsync(999);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public async Task GetAllForAdminAsync_ReturnsPaginatedProductsIncludingDeleted()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Cat1" };
            var products = new List<Product>
            {
                new Product {Id = 1, Name = "P1", Category = category, CategoryId = 1, IsDeleted = false, Description = "Test Description", ImageUrl = "/images/test.jpg"},
                new Product { Id = 2, Name = "P2", Category = category, CategoryId = 1, IsDeleted = true, Description="sussy baka", ImageUrl="Siwakow" }
            };
            var service = GetServiceWithSeededDb(products, new List<Category> { category });

            // Act
            var result = await service.GetAllForAdminAsync(1);

            // Assert
            Assert.Equal(2, result.Products.Count());
        }

        [Fact]
        public async Task UndeleteAsync_RestoresProductSuccessfully()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Cat1" };
            var product = new Product
            {
                Id = 1,
                Name = "P1",
                Category = category,
                CategoryId = 1,
                IsDeleted = true,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var service = GetServiceWithSeededDb(new List<Product> { product }, new List<Category> { category });

            // Act
            var (success, message) = await service.UndeleteAsync(1);

            // Assert
            Assert.True(success);
        }

        [Fact]
        public async Task UndeleteAsync_ReturnsFailure_WhenProductNotFound()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var (success, message) = await service.UndeleteAsync(999);

            // Assert
            Assert.False(success);
        }

        [Fact]
        public async Task UndeleteAsync_ReturnsFailure_WhenProductNotDeleted()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Cat1" };
            var product = new Product
            {
                Id = 1,
                Name = "P1",
                Category = category,
                CategoryId = 1,
                IsDeleted = false,
                Description = "Test Description",
                ImageUrl = "/images/test.jpg"
            };
            var service = GetServiceWithSeededDb(new List<Product> { product }, new List<Category> { category });

            // Act
            var (success, message) = await service.UndeleteAsync(1);

            // Assert
            Assert.False(success);
        }
    }
}
