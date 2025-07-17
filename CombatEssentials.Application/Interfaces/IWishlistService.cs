using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Application.DTOs.ProductDtos;

namespace CombatEssentials.Application.Interfaces
{
    public interface IWishlistService
    {
        Task<IEnumerable<ProductDto>> GetWishlistAsync(string userId, int page);
        Task<(bool Success, string Message)> AddToWishlistAsync(string userId, int productId);
        Task<(bool Success, string Message)> RemoveFromWishlistAsync(string userId, int productId);
    }
}
