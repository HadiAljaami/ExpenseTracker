using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepo;
    public CategoryService(ICategoryRepository categoryRepo) => _categoryRepo = categoryRepo;

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepo.GetAllAsync();
        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(int id)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Category", id);
        return MapToDto(category);
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        var category = new Category { Name = dto.Name, Icon = dto.Icon, Color = dto.Color };
        await _categoryRepo.AddAsync(category);
        await _categoryRepo.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task<CategoryDto> UpdateAsync(int id, CreateCategoryDto dto)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Category", id);

        category.Name = dto.Name;
        category.Icon = dto.Icon;
        category.Color = dto.Color;
        category.UpdatedAt = DateTime.UtcNow;

        await _categoryRepo.UpdateAsync(category);
        await _categoryRepo.SaveChangesAsync();
        return MapToDto(category);
    }

    public async Task DeleteAsync(int id)
    {
        var category = await _categoryRepo.GetByIdAsync(id)
            ?? throw new NotFoundException("Category", id);
        await _categoryRepo.DeleteAsync(category);
        await _categoryRepo.SaveChangesAsync();
    }

    private static CategoryDto MapToDto(Category c) => new()
    {
        Id = c.Id, Name = c.Name, Icon = c.Icon, Color = c.Color
    };
}
