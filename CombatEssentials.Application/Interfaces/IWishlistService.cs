using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Application.DTOs;

namespace CombatEssentials.Application.Interfaces
{
    public interface IWishlistService
    {
        Task<IEnumerable<ProductDto>> GetWishlistAsync(string userId);
        Task AddToWishlistAsync(string userId, int productId);
        Task RemoveFromWishlistAsync(string userId, int productId);
    }
}
