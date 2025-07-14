using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatEssentials.Application.DTOs.OrderDtos
{
    public class CreateOrderDto
    {
        public string ShippingAddress { get; set; }
        public List<CreateOrderItemDto> OrderItems { get; set; }
    }
}
