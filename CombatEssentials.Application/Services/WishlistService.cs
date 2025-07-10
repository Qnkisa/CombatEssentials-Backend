using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Application.DTOs;
using CombatEssentials.Application.Interfaces;
using CombatEssentials.Domain.Entities;
using CombatEssentials.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CombatEssentials.Application.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly ApplicationDbContext _context;
        public WishlistService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<ProductDto>> GetWishlistAsync(string userId)
        {
            return await _context.Wishlists
                .Where(w => w.UserId == userId)
                .Select(w => new ProductDto
                {
                    Id = w.Product.Id,
                    Name = w.Product.Name,
                    Price = w.Product.Price,
                    ImageUrl = w.Product.ImageUrl,
                    CategoryName = w.Product.Category.Name
                })
                .ToListAsync();
        }

        public async Task AddToWishlistAsync(string userId, int productId)
        {
            if (!await _context.Wishlists.AnyAsync(w => w.UserId == userId && w.ProductId == productId))
            {
                _context.Wishlists.Add(new Wishlist { UserId = userId, ProductId = productId });
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromWishlistAsync(string userId, int productId)
        {
            var item = await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);

            if (item != null)
            {
                _context.Wishlists.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }

}
