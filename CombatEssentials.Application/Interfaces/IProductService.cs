using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatEssentials.Application.DTOs.ProductDtos;

namespace CombatEssentials.Application.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync(int page, int? categoryId = null, string? name = null);
        Task<IEnumerable<ProductDto>> GetRandomProductsAsync(int count = 9);
        Task<ProductDto?> GetByIdAsync(int id);
        Task<(bool Success, string Message, ProductDto? CreatedProduct)> CreateAsync(CreateProductDto dto);
        Task<(bool Success, string Message)> UpdateAsync(int id, UpdateProductDto dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
        Task<(bool Success, string Message)> UndeleteAsync(int id);
        Task<IEnumerable<ProductDto>> GetAllForAdminAsync(int page, int? categoryId = null, string? name = null);
    }
}
