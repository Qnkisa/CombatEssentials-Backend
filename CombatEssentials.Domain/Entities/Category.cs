﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Domain.Validations;

namespace CombatEssentials.Domain.Entities
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(CategoryValidations.NameMaxLength)]
        public string Name { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
