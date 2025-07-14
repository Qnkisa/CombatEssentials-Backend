using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Application.DTOs.ReviewDtos;
using CombatEssentials.Domain.Entities;

namespace CombatEssentials.Application.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<GetReviewDto>> GetReviewsForProductAsync(int productId);
        Task AddReviewAsync(string userId, ReviewDto dto);
        Task<(bool Success, string Message)> DeleteReviewAsync(string userId, int reviewId);
    }

}
