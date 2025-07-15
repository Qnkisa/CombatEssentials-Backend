using CombatEssentials.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatEssentials.Application.DTOs.OrderDtos
{
    public class UpdateOrderDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string OrderStatus { get; set; }

        [Required]
        [MaxLength(200)]
        public string ShippingAddress { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        public List<CreateOrderItemDto> OrderItems { get; set; }
    }
}
