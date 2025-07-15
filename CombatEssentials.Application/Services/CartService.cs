using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Application.DTOs.CartDtos;
using CombatEssentials.Application.Interfaces;
using CombatEssentials.Domain.Entities;
using CombatEssentials.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CombatEssentials.Application.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        public CartService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<GetCartItemDto>> GetCartItemsAsync(string userId)
        {
            var shoppingCart = await _context.ShoppingCarts
                .Include(sc => sc.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(sc => sc.UserId == userId);

            if (shoppingCart == null)
                return new List<GetCartItemDto>();

            return shoppingCart.CartItems.Select(ci => new GetCartItemDto
            {
                Id = ci.Id,
                ShoppingCartId = ci.ShoppingCartId,
                ProductId = ci.ProductId,
                ProductName = ci.Product.Name,
                ProductPrice = ci.Product.Price,
                Quantity = ci.Quantity
            }).ToList();
        }

        public async Task<(bool Success, string Message)> AddToCartAsync(string userId, CartItemDto dto)
        {
            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                return (false, "Product not found.");

            if (dto.Quantity < 1)
                return (false, "Quantity must be at least 1.");

            var cart = await _context.ShoppingCarts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new ShoppingCart { UserId = userId, CartItems = new List<CartItem>() };
                _context.ShoppingCarts.Add(cart);
            }

            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == dto.ProductId);
            if (existingItem != null)
            {
                return (false, "Item is already in the cart.");
            }

            cart.CartItems.Add(new CartItem
            {
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            });

            await _context.SaveChangesAsync();
            return (true, "Item added to cart.");
        }

        public async Task<(bool Success, string Message)> RemoveFromCartAsync(string userId, int cartItemId)
        {
            var cartItem = await _context.CartItems
                .Include(ci => ci.ShoppingCart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.ShoppingCart.UserId == userId);

            if (cartItem == null)
                return (false, "Item not found or doesn't belong to the user.");

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return (true, "Item removed from cart.");
        }

        public async Task<(bool Success, string Message)> ClearCartAsync(string userId)
        {
            var cart = await _context.ShoppingCarts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null || !cart.CartItems.Any())
                return (false, "Cart is already empty.");

            _context.CartItems.RemoveRange(cart.CartItems);
            await _context.SaveChangesAsync();
            return (true, "Cart cleared.");
        }

        public async Task<(bool Success, string Message)> UpdateQuantityAsync(string userId, int cartItemId, int quantity)
        {
            if (quantity < 1)
                return (false, "Quantity must be at least 1.");

            var cartItem = await _context.CartItems
                .Include(ci => ci.ShoppingCart)
                .FirstOrDefaultAsync(ci => ci.Id == cartItemId && ci.ShoppingCart.UserId == userId);

            if (cartItem == null)
                return (false, "Cart item not found or doesn't belong to the user.");

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();
            return (true, "Quantity updated.");
        }
    }

}
