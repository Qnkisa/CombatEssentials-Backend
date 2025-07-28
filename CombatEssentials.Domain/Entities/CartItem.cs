using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CombatEssentials.Domain.Validations;

namespace CombatEssentials.Domain.Entities
{

    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ShoppingCartId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required, Range(CartItemValidations.QuantityMinValue, CartItemValidations.QuantityMaxValue)]
        public int Quantity { get; set; }

        [ForeignKey("ShoppingCartId")]
        public ShoppingCart ShoppingCart { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
