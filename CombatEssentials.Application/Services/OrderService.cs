using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Application.DTOs.OrderDtos;
using CombatEssentials.Application.Interfaces;
using CombatEssentials.Domain.Entities;
using CombatEssentials.Domain.Enums;
using CombatEssentials.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CombatEssentials.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderDto>> GetAllAsync()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ToListAsync();

            return orders.Select(MapToDto);
        }

        public async Task<OrderDto?> GetByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order == null ? null : MapToDto(order);
        }

        public async Task<IEnumerable<OrderDto>> GetByUserIdAsync(string userId)
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
                .ToListAsync();

            return orders.Select(MapToDto);
        }

        public async Task<OrderDto> CreateAsync(string? userId, CreateOrderDto dto)
        {
            var order = new Order
            {
                UserId = string.IsNullOrEmpty(userId) ? null : userId,
                OrderDate = DateTime.UtcNow,
                ShippingAddress = dto.ShippingAddress,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                OrderStatus = OrderStatus.Pending,
                OrderItems = new List<OrderItem>()
            };

            foreach (var itemDto in dto.OrderItems)
            {
                var product = await _context.Products.FindAsync(itemDto.ProductId);
                if (product == null) throw new Exception("Product not found");

                var total = product.Price * itemDto.Quantity;

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price,
                    TotalAmount = total
                });
            }

            order.TotalAmount = order.OrderItems.Sum(i => i.TotalAmount);

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var savedOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == order.Id);

            return MapToDto(savedOrder);
        }


        public async Task<(bool Success, string Message)> UpdateAsync(int id, UpdateOrderDto dto)
        {
            var existing = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == id);
            if (existing == null) return (false, "Order not found.");

            existing.FullName = dto.FullName;
            existing.PhoneNumber = dto.PhoneNumber;
            existing.ShippingAddress = dto.ShippingAddress;
            if (Enum.TryParse<OrderStatus>(dto.OrderStatus, true, out var parsedStatus))
                existing.OrderStatus = parsedStatus;

            _context.OrderItems.RemoveRange(existing.OrderItems);
            existing.OrderItems.Clear();

            foreach (var itemDto in dto.OrderItems)
            {
                var product = await _context.Products.FindAsync(itemDto.ProductId);
                if (product == null) continue;
                var total = product.Price * itemDto.Quantity;
                existing.OrderItems.Add(new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price,
                    TotalAmount = total
                });
            }
            existing.TotalAmount = existing.OrderItems.Sum(i => i.TotalAmount);

            await _context.SaveChangesAsync();
            return (true, "Order updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return (false, "Order not found.");

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return (true, "Order deleted successfully.");
        }

        // Helper
        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                FullName = order.FullName,
                PhoneNumber = order.PhoneNumber,
                OrderStatus = order.OrderStatus.ToString(),
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalAmount = oi.TotalAmount
                }).ToList()
            };
        }
    }
}
