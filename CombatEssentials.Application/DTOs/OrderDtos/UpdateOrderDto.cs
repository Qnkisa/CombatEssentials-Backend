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
    }
}
