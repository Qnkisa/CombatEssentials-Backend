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

    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required, Range(ReviewValidations.RatingMinValue, ReviewValidations.RatingMaxValue)]
        public int Rating { get; set; }

        [MaxLength(ReviewValidations.CommentMaxLength)]
        public string Comment { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
    }
}
