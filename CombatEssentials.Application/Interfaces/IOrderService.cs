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
        Task<IEnumerable<OrderDto>> GetAllAsync(int page);
        Task<IEnumerable<OrderDto>> GetByUserIdAsync(string userId, int page);
        Task<OrderDto> CreateAsync(string userId, CreateOrderDto createDto);
        Task<(bool Success, string Message)> UpdateAsync(int id, UpdateOrderDto dto);
    }
}
