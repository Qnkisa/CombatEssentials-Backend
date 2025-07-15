using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Application.DTOs.ProductDtos;
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
                .Include(w => w.Product)
                    .ThenInclude(p => p.Category)
                .Select(w => new ProductDto
                {
                    Id = w.Product.Id,
                    Name = w.Product.Name,
                    Price = w.Product.Price,
                    Description = w.Product.Description,
                    ImageUrl = w.Product.ImageUrl,
                    CategoryName = w.Product.Category.Name
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> AddToWishlistAsync(string userId, int productId)
        {
            if (string.IsNullOrEmpty(userId))
                return (false, "User is not authenticated.");

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return (false, "Product not found.");

            var exists = await _context.Wishlists.AnyAsync(w => w.UserId == userId && w.ProductId == productId);
            if (exists)
                return (false, "Product is already in the wishlist.");

            _context.Wishlists.Add(new Wishlist { UserId = userId, ProductId = productId });
            await _context.SaveChangesAsync();
            return (true, "Product added to wishlist.");
        }

        public async Task<(bool Success, string Message)> RemoveFromWishlistAsync(string userId, int productId)
        {
            if (string.IsNullOrEmpty(userId))
                return (false, "User is not authenticated.");

            var item = await _context.Wishlists.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
            if (item == null)
                return (false, "Product not found in wishlist.");

            _context.Wishlists.Remove(item);
            await _context.SaveChangesAsync();
            return (true, "Product removed from wishlist.");
        }
    }

}
