using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Domain.Validations;
using Microsoft.AspNetCore.Http;

namespace CombatEssentials.Application.DTOs.ProductDtos
{
    public class UpdateProductDto
    {
        [Required, MaxLength(ProductValidations.NameMaxLength)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(ProductValidations.DescriptionMaxLength)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public IFormFile ImageFile { get; set; }
    }
}
