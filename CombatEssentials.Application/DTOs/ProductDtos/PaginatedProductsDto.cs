using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatEssentials.Application.DTOs.ProductDtos
{
    public class PaginatedProductsDto
    {
        public IEnumerable<ProductDto> Products { get; set; } = new List<ProductDto>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int LastPage => (int)Math.Ceiling((double)TotalItems / PageSize);
    }

}
