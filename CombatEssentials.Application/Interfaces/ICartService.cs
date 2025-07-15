using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Application.DTOs.CartDtos;
using CombatEssentials.Domain.Entities;

namespace CombatEssentials.Application.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<GetCartItemDto>> GetCartItemsAsync(string userId);
        Task<(bool Success, string Message)> AddToCartAsync(string userId, CartItemDto dto);
        Task<(bool Success, string Message)> RemoveFromCartAsync(string userId, int cartItemId);
        Task<(bool Success, string Message)> ClearCartAsync(string userId);
        Task<(bool Success, string Message)> UpdateQuantityAsync(string userId, int cartItemId, int quantity);
    }
}
