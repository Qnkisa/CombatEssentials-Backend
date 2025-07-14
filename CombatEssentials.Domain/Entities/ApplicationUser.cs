using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Domain.Validations;
using Microsoft.AspNetCore.Identity;

namespace CombatEssentials.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // Custom fields
        [Required, MaxLength(ApplicationUserValidations.FirstNameMaxLength)]
        public string FirstName { get; set; }

        [Required, MaxLength(ApplicationUserValidations.LastNameMaxLength)]
        public string LastName { get; set; }

        // Navigation properties
        public ICollection<Wishlist> Wishlists { get; set; }
        public ICollection<ShoppingCart> ShoppingCarts { get; set; }
        public ICollection<Order> Orders { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
