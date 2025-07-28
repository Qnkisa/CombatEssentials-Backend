using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CombatEssentials.Domain.Enums;

namespace CombatEssentials.Domain.Entities
{

    public class Order
    {
        [Key]
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required]
        public DateTime OrderDate { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public OrderStatus OrderStatus { get; set; }

        [Required, MaxLength(200)]
        public string ShippingAddress { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, MaxLength(20)]
        public string PhoneNumber { get; set; }
        
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; }
    }

}
