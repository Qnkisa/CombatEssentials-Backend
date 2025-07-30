using CombatEssentials.Application.DTOs.CategoryDtos;
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
    public class CategoryServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public CategoryServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "CategoryServiceTestsDb")
                .Options;
        }

        private CategoryService GetServiceWithSeededDb(List<Category> categories = null)
        {
            var context = new ApplicationDbContext(_dbOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            
            if (categories != null)
            {
                context.Categories.AddRange(categories);
            }
            context.SaveChanges();
            return new CategoryService(context);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Boxing Equipment" },
                new Category { Id = 2, Name = "MMA Equipment" },
                new Category { Id = 3, Name = "Protective Gear" },
                new Category { Id = 4, Name = "Training Equipment" }
            };
            var service = GetServiceWithSeededDb(categories);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(4, result.Count());
            
            var categoryList = result.ToList();
            Assert.Equal(1, categoryList[0].Id);
            Assert.Equal("Boxing Equipment", categoryList[0].Name);
            Assert.Equal(2, categoryList[1].Id);
            Assert.Equal("MMA Equipment", categoryList[1].Name);
            Assert.Equal(3, categoryList[2].Id);
            Assert.Equal("Protective Gear", categoryList[2].Name);
            Assert.Equal(4, categoryList[3].Id);
            Assert.Equal("Training Equipment", categoryList[3].Name);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmpty_WhenNoCategories()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsSingleCategory()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Boxing Equipment" }
            };
            var service = GetServiceWithSeededDb(categories);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            var category = result.First();
            Assert.Equal(1, category.Id);
            Assert.Equal("Boxing Equipment", category.Name);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsCategoriesInCorrectOrder()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 3, Name = "Protective Gear" },
                new Category { Id = 1, Name = "Boxing Equipment" },
                new Category { Id = 2, Name = "MMA Equipment" }
            };
            var service = GetServiceWithSeededDb(categories);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            
            // Categories should be returned in the order they were added to the database
            var categoryList = result.ToList();
            Assert.Equal(3, categoryList[0].Id);
            Assert.Equal("Protective Gear", categoryList[0].Name);
            Assert.Equal(1, categoryList[1].Id);
            Assert.Equal("Boxing Equipment", categoryList[1].Name);
            Assert.Equal(2, categoryList[2].Id);
            Assert.Equal("MMA Equipment", categoryList[2].Name);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsCorrectDtoStructure()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Test Category" }
            };
            var service = GetServiceWithSeededDb(categories);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var category = result.First();
            
            // Verify the DTO structure matches CategoryDto
            Assert.IsType<CategoryDto>(category);
            Assert.Equal(1, category.Id);
            Assert.Equal("Test Category", category.Name);
        }

        [Fact]
        public async Task GetAllAsync_HandlesSpecialCharactersInNames()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Boxing & MMA Equipment" },
                new Category { Id = 2, Name = "Protective Gear (Head & Body)" },
                new Category { Id = 3, Name = "Training Equipment - Heavy Bags" }
            };
            var service = GetServiceWithSeededDb(categories);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            
            var categoryList = result.ToList();
            Assert.Equal("Boxing & MMA Equipment", categoryList[0].Name);
            Assert.Equal("Protective Gear (Head & Body)", categoryList[1].Name);
            Assert.Equal("Training Equipment - Heavy Bags", categoryList[2].Name);
        }

        [Fact]
        public async Task GetAllAsync_HandlesEmptyCategoryNames()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "" },
                new Category { Id = 2, Name = "   " },
                new Category { Id = 3, Name = "Valid Category" }
            };
            var service = GetServiceWithSeededDb(categories);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
            
            var categoryList = result.ToList();
            Assert.Equal("", categoryList[0].Name);
            Assert.Equal("   ", categoryList[1].Name);
            Assert.Equal("Valid Category", categoryList[2].Name);
        }
    }
}
