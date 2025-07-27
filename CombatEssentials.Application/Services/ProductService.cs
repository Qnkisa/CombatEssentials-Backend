using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Application.DTOs.ProductDtos;
using CombatEssentials.Application.Interfaces;
using CombatEssentials.Domain.Entities;
using CombatEssentials.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CombatEssentials.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductService(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<PaginatedProductsDto> GetAllAsync(int page, int? categoryId = null, string? name = null)
        {
            const int pageSize = 15;

            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted);

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));
            }

            var totalItems = await query.CountAsync();

            var products = await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            return new PaginatedProductsDto
            {
                Products = products,
                CurrentPage = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }


        public async Task<IEnumerable<ProductDto>> GetRandomProductsAsync(int count = 9)
        {
            var totalProducts = await _context.Products.CountAsync();
            var takeCount = Math.Min(count, totalProducts);

            return await _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted)
                .OrderBy(p => Guid.NewGuid())
                .Take(takeCount)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Where(p => !p.IsDeleted && p.Id == id)
                .FirstOrDefaultAsync();

            if (product == null) return null;

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                ImageUrl = product.ImageUrl
            };
        }

        public async Task<(bool Success, string Message, ProductDto? CreatedProduct)> CreateAsync(CreateProductDto dto)
        {
            var imageUrl = await SaveImageAsync(dto.ImageFile);

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                ImageUrl = imageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var created = await GetByIdAsync(product.Id);
            if (created == null)
                return (false, "Product creation failed.", null);

            return (true, "Product created successfully.", created);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return (false, $"Product with ID {id} not found.");

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.CategoryId = dto.CategoryId;

            if (dto.ImageFile != null)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    var oldImagePath = Path.Combine(_env.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                var imageUrl = await SaveImageAsync(dto.ImageFile);
                product.ImageUrl = imageUrl;
            }

            await _context.SaveChangesAsync();
            return (true, "Product updated successfully.");
        }


        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return (false, $"Product with ID {id} not found.");

            product.IsDeleted = true;
            await _context.SaveChangesAsync();
            return (true, "Product deleted successfully.");
        }

        public async Task<IEnumerable<ProductDto>> GetAllForAdminAsync(int page, int? categoryId = null, string? name = null)
        {
            const int pageSize = 15;
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(p => p.Name.ToLower().Contains(name.ToLower()));
            }

            return await query
                .OrderBy(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    ImageUrl = p.ImageUrl,
                    IsDeleted = p.IsDeleted
                })
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> UndeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return (false, $"Product with ID {id} not found.");

            if (!product.IsDeleted)
                return (false, "Product is not deleted.");

            product.IsDeleted = false;
            await _context.SaveChangesAsync();

            return (true, "Product restored successfully.");
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var folder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{fileName}";
        }
    }
}
