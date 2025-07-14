using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Application.DTOs.OrderDtos;
using CombatEssentials.Domain.Entities;

namespace CombatEssentials.Application.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllAsync();
        Task<OrderDto?> GetByIdAsync(int id);
        Task<IEnumerable<OrderDto>> GetByUserIdAsync(string userId);
        Task<OrderDto> CreateAsync(string userId, CreateOrderDto createDto);
        Task<bool> UpdateAsync(int id, string shippingAddress, string status);
        Task<bool> DeleteAsync(int id);
    }
}
