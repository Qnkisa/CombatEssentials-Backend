using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CombatEssentials.Domain.Entities
{

    public class Wishlist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public int ProductId { get; set; }

        // Navigation
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
