using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Application.DTOs.ReviewDtos;
using CombatEssentials.Application.Interfaces;
using CombatEssentials.Domain.Entities;
using CombatEssentials.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CombatEssentials.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;
        public ReviewService(ApplicationDbContext context) => _context = context;

        public async Task<IEnumerable<GetReviewDto>> GetReviewsForProductAsync(int productId, int page)
        {
            const int pageSize = 15;
            return await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Include(r => r.User)
                .OrderByDescending(r => r.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new GetReviewDto
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User.UserName,
                    ProductId = r.ProductId,
                    Rating = r.Rating,
                    Comment = r.Comment
                })
                .ToListAsync();
        }

        public async Task AddReviewAsync(string userId, ReviewDto dto)
        {
            var review = new Review
            {
                UserId = userId,
                ProductId = dto.ProductId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };
            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
        }

        public async Task<(bool Success, string Message)> DeleteReviewAsync(string userId, int reviewId)
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.Id == reviewId);

            if (review == null)
                return (false, "Review not found.");

            if (review.UserId != userId)
                return (false, "You are not authorized to delete this review.");

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return (true, "Review deleted successfully.");
        }

        public async Task<double> GetAverageRatingForProductAsync(int productId)
        {
            return await _context.Reviews
                .Where(r => r.ProductId == productId)
                .Select(r => (double?)r.Rating)
                .AverageAsync() ?? 0.0;
        }
    }
}
