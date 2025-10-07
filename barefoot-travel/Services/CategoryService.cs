using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Category;
using barefoot_travel.Models;
using barefoot_travel.Repositories;

namespace barefoot_travel.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<ApiResponse> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return new ApiResponse(false, "Category not found");
                }

                var categoryDto = MapToCategoryDto(category);
                return new ApiResponse(true, "Category retrieved successfully", categoryDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving category: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                var categoryDtos = categories.Select(MapToCategoryDto).ToList();
                return new ApiResponse(true, "Categories retrieved successfully", categoryDtos);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving categories: {ex.Message}");
            }
        }

        public async Task<PagedResult<CategoryDto>> GetCategoriesPagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", string? type = null, bool? active = null)
        {
            try
            {
                var pagedResult = await _categoryRepository.GetPagedAsync(page, pageSize, sortBy, sortOrder, type, active);
                var categoryDtos = pagedResult.Items.Select(MapToCategoryDto).ToList();

                return new PagedResult<CategoryDto>
                {
                    Items = categoryDtos,
                    TotalItems = pagedResult.TotalItems,
                    TotalPages = pagedResult.TotalPages,
                    CurrentPage = pagedResult.CurrentPage,
                    PageSize = pagedResult.PageSize
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving paged categories: {ex.Message}");
            }
        }

        public async Task<ApiResponse> CreateCategoryAsync(CreateCategoryDto dto, string adminUsername)
        {
            try
            {
                // Validate name uniqueness
                if (await _categoryRepository.NameExistsAsync(dto.CategoryName))
                {
                    return new ApiResponse(false, "Category name already exists");
                }

                // Validate parent exists if provided
                if (dto.ParentId.HasValue && !await _categoryRepository.ExistsAsync(dto.ParentId.Value))
                {
                    return new ApiResponse(false, "Parent category not found");
                }

                var category = MapToCategory(dto, adminUsername);
                var createdCategory = await _categoryRepository.CreateAsync(category);
                var categoryDto = MapToCategoryDto(createdCategory);

                return new ApiResponse(true, "Category created successfully", categoryDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error creating category: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateCategoryAsync(int id, UpdateCategoryDto dto, string adminUsername)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                {
                    return new ApiResponse(false, "Category not found");
                }

                // Validate name uniqueness (excluding current category)
                if (await _categoryRepository.NameExistsAsync(dto.CategoryName, id))
                {
                    return new ApiResponse(false, "Category name already exists");
                }

                // Validate parent exists if provided
                if (dto.ParentId.HasValue && !await _categoryRepository.ExistsAsync(dto.ParentId.Value))
                {
                    return new ApiResponse(false, "Parent category not found");
                }

                // Prevent setting parent to self
                if (dto.ParentId == id)
                {
                    return new ApiResponse(false, "Category cannot be its own parent");
                }

                MapToCategoryForUpdate(category, dto, adminUsername);
                await _categoryRepository.UpdateAsync(category);
                var categoryDto = MapToCategoryDto(category);

                return new ApiResponse(true, "Category updated successfully", categoryDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error updating category: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteCategoryAsync(int id, string adminUsername)
        {
            try
            {
                // Check if category has children
                if (await _categoryRepository.HasChildrenAsync(id))
                {
                    return new ApiResponse(false, "Cannot delete category with child categories");
                }

                var success = await _categoryRepository.DeleteAsync(id);
                if (!success)
                {
                    return new ApiResponse(false, "Category not found");
                }

                return new ApiResponse(true, "Category deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error deleting category: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateCategoryStatusAsync(int id, bool active, string adminUsername)
        {
            try
            {
                var success = await _categoryRepository.UpdateStatusAsync(id, active, adminUsername);
                if (!success)
                {
                    return new ApiResponse(false, "Category not found");
                }

                var statusDto = new CategoryStatusDto { Id = id, Active = active, UpdatedTime = DateTime.UtcNow };
                return new ApiResponse(true, "Category status updated successfully", statusDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error updating category status: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetCategoriesByTypeAsync(string type)
        {
            try
            {
                var categories = await _categoryRepository.GetByTypeAsync(type);
                var categoryDtos = categories.Select(MapToCategoryDto).ToList();
                return new ApiResponse(true, "Categories retrieved successfully", categoryDtos);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving categories: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetCategoriesByParentIdAsync(int? parentId)
        {
            try
            {
                var categories = await _categoryRepository.GetByParentIdAsync(parentId);
                var categoryDtos = categories.Select(MapToCategoryDto).ToList();
                return new ApiResponse(true, "Categories retrieved successfully", categoryDtos);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving categories: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetAllType() 
        {
            try
            {
                var types = await _categoryRepository.GetAllType();
                return new ApiResponse(true, "Category types retrieved successfully", types);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving category types: {ex.Message}");
            }
        }

        #region Private Helper Methods

        private Category MapToCategory(CreateCategoryDto dto, string adminUsername)
        {
            return new Category
            {
                CategoryName = dto.CategoryName,
                ParentId = dto.ParentId,
                Enable = dto.Enable,
                Type = dto.Type,
                Priority = dto.Priority,
                CreatedTime = DateTime.UtcNow,
                UpdatedBy = adminUsername,
                Active = true
            };
        }

        private void MapToCategoryForUpdate(Category category, UpdateCategoryDto dto, string adminUsername)
        {
            category.CategoryName = dto.CategoryName;
            category.ParentId = dto.ParentId;
            category.Enable = dto.Enable;
            category.Type = dto.Type;
            category.Priority = dto.Priority;
            category.UpdatedTime = DateTime.UtcNow;
            category.UpdatedBy = adminUsername;
        }

        private CategoryDto MapToCategoryDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                ParentId = category.ParentId,
                CategoryName = category.CategoryName,
                Enable = category.Enable,
                Type = category.Type,
                Priority = category.Priority,
                CreatedTime = category.CreatedTime,
                UpdatedTime = category.UpdatedTime,
                UpdatedBy = category.UpdatedBy,
                Active = category.Active
            };
        }

        #endregion
    }
}
