using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatEssentials.Application.DTOs.OrderDtos
{
    public class PaginatedOrderDto
    {
        public int TotalOrders { get; set; }
        public IEnumerable<OrderDto> Orders { get; set; } = new List<OrderDto>();
    }
}
