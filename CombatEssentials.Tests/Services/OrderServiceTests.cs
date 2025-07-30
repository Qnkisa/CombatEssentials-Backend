using CombatEssentials.Application.DTOs.OrderDtos;
using CombatEssentials.Application.Services;
using CombatEssentials.Domain.Entities;
using CombatEssentials.Domain.Enums;
using CombatEssentials.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CombatEssentials.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public OrderServiceTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "OrderServiceTestsDb")
                .Options;
        }

        private OrderService GetServiceWithSeededDb(List<Order> orders = null, List<Product> products = null)
        {
            var context = new ApplicationDbContext(_dbOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            
            if (products != null)
            {
                context.Products.AddRange(products);
            }
            if (orders != null)
            {
                context.Orders.AddRange(orders);
            }
            context.SaveChanges();
            return new OrderService(context);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsPaginatedOrders()
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
            var orders = Enumerable.Range(1, 20).Select(i => new Order
            {
                Id = i,
                UserId = $"user{i % 3}",
                OrderDate = DateTime.UtcNow.AddDays(-i),
                TotalAmount = 10.0m * i,
                ShippingAddress = $"Address {i}",
                FullName = $"User {i}",
                PhoneNumber = $"123-456-{i:D4}",
                OrderStatus = OrderStatus.Pending,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = 1,
                        Quantity = i,
                        UnitPrice = 10.0m,
                        TotalAmount = 10.0m * i,
                        Product = product
                    }
                }
            }).ToList();
            var service = GetServiceWithSeededDb(orders, new List<Product> { product });

            // Act
            var result = await service.GetAllAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20, result.TotalOrders);
            Assert.Equal(15, result.Orders.Count()); // pageSize = 15
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmpty_WhenNoOrders()
        {
            // Arrange
            var service = GetServiceWithSeededDb();

            // Act
            var result = await service.GetAllAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalOrders);
            Assert.Empty(result.Orders);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsUserOrders()
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
            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1,
                    UserId = "user1",
                    OrderDate = DateTime.UtcNow.AddDays(-1),
                    TotalAmount = 20.0m,
                    ShippingAddress = "Address 1",
                    FullName = "User 1",
                    PhoneNumber = "123-456-0001",
                    OrderStatus = OrderStatus.Pending,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId = 1,
                            Quantity = 2,
                            UnitPrice = 10.0m,
                            TotalAmount = 20.0m,
                            Product = product
                        }
                    }
                },
                new Order
                {
                    Id = 2,
                    UserId = "user2",
                    OrderDate = DateTime.UtcNow.AddDays(-2),
                    TotalAmount = 30.0m,
                    ShippingAddress = "Address 2",
                    FullName = "User 2",
                    PhoneNumber = "123-456-0002",
                    OrderStatus = OrderStatus.Delivered,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId = 1,
                            Quantity = 3,
                            UnitPrice = 10.0m,
                            TotalAmount = 30.0m,
                            Product = product
                        }
                    }
                }
            };
            var service = GetServiceWithSeededDb(orders, new List<Product> { product });

            // Act
            var result = await service.GetByUserIdAsync("user1", 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalOrders);
            Assert.Single(result.Orders);
            Assert.Equal("user1", result.Orders.First().UserId);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsEmpty_WhenUserHasNoOrders()
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
            var order = new Order
            {
                Id = 1,
                UserId = "user1",
                OrderDate = DateTime.UtcNow,
                TotalAmount = 10.0m,
                ShippingAddress = "Address 1",
                FullName = "User 1",
                PhoneNumber = "123-456-0001",
                OrderStatus = OrderStatus.Pending,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = 1,
                        Quantity = 1,
                        UnitPrice = 10.0m,
                        TotalAmount = 10.0m,
                        Product = product
                    }
                }
            };
            var service = GetServiceWithSeededDb(new List<Order> { order }, new List<Product> { product });

            // Act
            var result = await service.GetByUserIdAsync("user2", 1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.TotalOrders);
            Assert.Empty(result.Orders);
        }

        [Fact]
        public async Task CreateAsync_CreatesOrderSuccessfully()
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
            var dto = new CreateOrderDto
            {
                ShippingAddress = "123 Test St",
                FullName = "John Doe",
                PhoneNumber = "123-456-7890",
                OrderItems = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { ProductId = 1, Quantity = 2 }
                }
            };

            // Act
            var result = await service.CreateAsync("user1", dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("user1", result.UserId);
            Assert.Equal("123 Test St", result.ShippingAddress);
            Assert.Equal("John Doe", result.FullName);
            Assert.Equal("123-456-7890", result.PhoneNumber);
            Assert.Equal("Pending", result.OrderStatus);
            Assert.Equal(20.0m, result.TotalAmount); // 10.0 * 2
            Assert.Single(result.OrderItems);
            Assert.Equal(1, result.OrderItems.First().ProductId);
            Assert.Equal(2, result.OrderItems.First().Quantity);
            Assert.Equal(10.0m, result.OrderItems.First().UnitPrice);
            Assert.Equal(20.0m, result.OrderItems.First().TotalAmount);
        }

        [Fact]
        public async Task CreateAsync_CreatesOrderWithoutUserId()
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
            var dto = new CreateOrderDto
            {
                ShippingAddress = "123 Test St",
                FullName = "John Doe",
                PhoneNumber = "123-456-7890",
                OrderItems = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { ProductId = 1, Quantity = 1 }
                }
            };

            // Act
            var result = await service.CreateAsync(null, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.UserId);
            Assert.Equal("Pending", result.OrderStatus);
        }

        [Fact]
        public async Task CreateAsync_ThrowsException_WhenProductNotFound()
        {
            // Arrange
            var service = GetServiceWithSeededDb();
            var dto = new CreateOrderDto
            {
                ShippingAddress = "123 Test St",
                FullName = "John Doe",
                PhoneNumber = "123-456-7890",
                OrderItems = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { ProductId = 999, Quantity = 1 }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => service.CreateAsync("user1", dto));
        }

        [Fact]
        public async Task CreateAsync_CreatesOrderWithMultipleItems()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Price = 10.0m, ImageUrl="url", Description="test" },
                new Product { Id = 2, Name = "Product 2", Price = 15.0m, ImageUrl="sussy", Description="baka"  }
            };
            var service = GetServiceWithSeededDb(null, products);
            var dto = new CreateOrderDto
            {
                ShippingAddress = "123 Test St",
                FullName = "John Doe",
                PhoneNumber = "123-456-7890",
                OrderItems = new List<CreateOrderItemDto>
                {
                    new CreateOrderItemDto { ProductId = 1, Quantity = 2 },
                    new CreateOrderItemDto { ProductId = 2, Quantity = 1 }
                }
            };

            // Act
            var result = await service.CreateAsync("user1", dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(35.0m, result.TotalAmount); // (10 * 2) + (15 * 1)
            Assert.Equal(2, result.OrderItems.Count);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesOrderStatusSuccessfully()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "user1",
                OrderDate = DateTime.UtcNow,
                TotalAmount = 10.0m,
                ShippingAddress = "Address 1",
                FullName = "User 1",
                PhoneNumber = "123-456-0001",
                OrderStatus = OrderStatus.Pending
            };
            var service = GetServiceWithSeededDb(new List<Order> { order });
            var dto = new UpdateOrderDto { OrderStatus = "Delivered" };

            // Act
            var (success, message) = await service.UpdateAsync(1, dto);

            // Assert
            Assert.True(success);
            Assert.Equal("Order status updated successfully.", message);
            
            var context = new ApplicationDbContext(_dbOptions);
            var updatedOrder = await context.Orders.FindAsync(1);
            Assert.Equal(OrderStatus.Delivered, updatedOrder.OrderStatus);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsFailure_WhenOrderNotFound()
        {
            // Arrange
            var service = GetServiceWithSeededDb();
            var dto = new UpdateOrderDto { OrderStatus = "Completed" };

            // Act
            var (success, message) = await service.UpdateAsync(999, dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Order not found.", message);
        }

        [Fact]
        public async Task UpdateAsync_ReturnsFailure_WhenInvalidStatus()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "user1",
                OrderDate = DateTime.UtcNow,
                TotalAmount = 10.0m,
                ShippingAddress = "Address 1",
                FullName = "User 1",
                PhoneNumber = "123-456-0001",
                OrderStatus = OrderStatus.Pending
            };
            var service = GetServiceWithSeededDb(new List<Order> { order });
            var dto = new UpdateOrderDto { OrderStatus = "InvalidStatus" };

            // Act
            var (success, message) = await service.UpdateAsync(1, dto);

            // Assert
            Assert.False(success);
            Assert.Equal("Invalid order status.", message);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesWithValidStatuses()
        {
            // Arrange
            var order = new Order
            {
                Id = 1,
                UserId = "user1",
                OrderDate = DateTime.UtcNow,
                TotalAmount = 10.0m,
                ShippingAddress = "Address 1",
                FullName = "User 1",
                PhoneNumber = "123-456-0001",
                OrderStatus = OrderStatus.Pending
            };
            var service = GetServiceWithSeededDb(new List<Order> { order });

            // Test different valid statuses
            var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };

            foreach (var status in validStatuses)
            {
                var dto = new UpdateOrderDto { OrderStatus = status };
                var (success, message) = await service.UpdateAsync(1, dto);
                
                Assert.True(success);
                Assert.Equal("Order status updated successfully.", message);
            }
        }
    }
}
