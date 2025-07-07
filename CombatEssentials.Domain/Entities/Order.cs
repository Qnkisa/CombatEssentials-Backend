using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CombatEssentials.Domain.Entities
{

    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        [Required]
        public string OrderStatus { get; set; }
        [Required]
        public string ShippingAddress { get; set; }

        // Navigation
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }
}
