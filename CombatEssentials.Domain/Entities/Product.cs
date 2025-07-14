using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CombatEssentials.Domain.Validations;

namespace CombatEssentials.Domain.Entities
{

    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(ProductValidations.NameMaxLength)]
        public string Name { get; set; }

        [MaxLength(ProductValidations.DescriptionMaxLength)]
        public string Description { get; set; }
        [Required]
        public string ImageUrl { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        // Navigation
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        public ICollection<Review> Reviews { get; set; }
        public ICollection<CartItem> CartItems { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; }
        public ICollection<Wishlist> Wishlists { get; set; }
    }
}
