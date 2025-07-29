using CombatEssentials.Application.DTOs.ProductDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatEssentials.Application.DTOs.WishlistDtos
{
    public class PaginatedWishlistDto
    {
        public int TotalCount { get; set; }
        public IEnumerable<ProductDto> Products { get; set; } = new List<ProductDto>();
    }
}
