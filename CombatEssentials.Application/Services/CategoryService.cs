using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Application.DTOs.CategoryDtos;
using CombatEssentials.Application.Interfaces;
using CombatEssentials.Domain.Entities;
using CombatEssentials.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CombatEssentials.Application.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }
    }
}
