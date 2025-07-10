using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Application.DTOs;
using CombatEssentials.Domain.Entities;

namespace CombatEssentials.Application.Interfaces
{
    public interface ICartService
    {
        Task<IEnumerable<CartItem>> GetCartItemsAsync(string userId);
        Task AddToCartAsync(string userId, CartItemDto dto);
        Task RemoveFromCartAsync(string userId, int productId);
        Task ClearCartAsync(string userId);
    }
}
