using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Application.DTOs;
using CombatEssentials.Domain.Entities;

namespace CombatEssentials.Application.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetReviewsForProductAsync(int productId);
        Task AddReviewAsync(string userId, ReviewDto dto);
        Task DeleteReviewAsync(string userId, int reviewId);
    }

}
