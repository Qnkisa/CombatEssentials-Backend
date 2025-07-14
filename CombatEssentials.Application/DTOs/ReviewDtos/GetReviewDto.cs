using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatEssentials.Application.DTOs.ReviewDtos
{
    public class GetReviewDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string? UserName { get; set; }
        public int ProductId { get; set; }
        public int Rating { get; set; }
        public string Comment { get; set; }
    }
}
