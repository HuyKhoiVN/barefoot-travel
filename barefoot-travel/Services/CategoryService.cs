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
        private readonly ITourRepository _tourRepository;

        public CategoryService(ICategoryRepository categoryRepository, ITourRepository tourRepository)
        {
            _categoryRepository = categoryRepository;
            _tourRepository = tourRepository;
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

                var categoryDto = await MapToCategoryDto(category);
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
                var categoryDtos = await MapToCategoryDtos(categories);
                return new ApiResponse(true, "Categories retrieved successfully", categoryDtos);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving categories: {ex.Message}");
            }
        }

        public async Task<PagedResult<CategoryDto>> GetCategoriesPagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", string? categoryName = null, string? type = null, List<int>? categoryIds = null, bool? active = null)
        {
            try
            {
                var pagedResult = await _categoryRepository.GetPagedAsync(page, pageSize, sortBy, sortOrder, categoryName, type, categoryIds, active);
                var categoryDtos = await MapToCategoryDtos(pagedResult.Items);

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

                // Generate slug if not provided
                var slug = string.IsNullOrWhiteSpace(dto.Slug) 
                    ? SlugGenerator.GenerateSlug(dto.CategoryName) 
                    : SlugGenerator.GenerateSlug(dto.Slug);

                // Only set slug if generation was successful
                if (!string.IsNullOrWhiteSpace(slug))
                {
                    // Validate slug format
                    if (!SlugGenerator.IsValidSlug(slug))
                    {
                        return new ApiResponse(false, "Invalid slug format");
                    }

                    // Ensure slug is unique
                    if (await _categoryRepository.SlugExistsAsync(slug))
                    {
                        var existingSlugs = await _categoryRepository.GetAllSlugsAsync();
                        slug = SlugGenerator.EnsureUnique(slug, existingSlugs);
                    }
                }

                var category = MapToCategory(dto, adminUsername);
                category.Slug = string.IsNullOrWhiteSpace(slug) ? null : slug;
                var createdCategory = await _categoryRepository.CreateAsync(category);
                var categoryDto = await MapToCategoryDto(createdCategory);

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

                // Handle slug update
                if (!string.IsNullOrWhiteSpace(dto.Slug))
                {
                    var newSlug = SlugGenerator.GenerateSlug(dto.Slug);
                    
                    // Only update if slug generation was successful
                    if (!string.IsNullOrWhiteSpace(newSlug))
                    {
                        // Validate slug format
                        if (!SlugGenerator.IsValidSlug(newSlug))
                        {
                            return new ApiResponse(false, "Invalid slug format");
                        }
                        
                        // Check if slug changed and new slug is unique
                        if (newSlug != category.Slug && 
                            await _categoryRepository.SlugExistsAsync(newSlug, id))
                        {
                            return new ApiResponse(false, "Slug already exists");
                        }
                        
                        category.Slug = newSlug;
                    }
                }
                else if (dto.CategoryName != category.CategoryName)
                {
                    // Auto-update slug if name changed but slug not provided
                    var newSlug = SlugGenerator.GenerateSlug(dto.CategoryName);
                    if (!string.IsNullOrWhiteSpace(newSlug) && 
                        !await _categoryRepository.SlugExistsAsync(newSlug, id))
                    {
                        category.Slug = newSlug;
                    }
                }

                MapToCategoryForUpdate(category, dto, adminUsername);
                await _categoryRepository.UpdateAsync(category);
                var categoryDto = await MapToCategoryDto(category);

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
                var categoryDtos = await MapToCategoryDtos(categories);
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
                var categoryDtos = await MapToCategoryDtos(categories);
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

        public async Task<ApiResponse> GetCategoryTreeAsync()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                var categoryDtos = await MapToCategoryDtos(categories);
                
                // Get total tours for each category
                foreach (var categoryDto in categoryDtos)
                {
                    categoryDto.TotalTours = await _tourRepository.GetTourCountByCategoryAsync(categoryDto.Id);
                }
                
                var categoryTree = BuildCategoryTree(categoryDtos, null);
                return new ApiResponse(true, "Category tree retrieved successfully", categoryTree);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving category tree: {ex.Message}");
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

        public async Task<PagedResult<CategoryDto>> GetTreePagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", string? categoryName = null, string? type = null, List<int>? categoryIds = null, bool? active = null)
        {
            try
            {
                var pagedResult = await _categoryRepository.GetTreePagedAsync(page, pageSize, sortBy, sortOrder, categoryName, type, categoryIds, active);
                var categoryDtos = await MapToCategoryDtos(pagedResult.Items);

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
                throw new Exception($"Error retrieving paged category tree: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetChildrenAsync(int parentId)
        {
            try
            {
                var children = await _categoryRepository.GetChildrenAsync(parentId);
                var categoryDtos = await MapToCategoryDtos(children);
                return new ApiResponse(true, "Category children retrieved successfully", categoryDtos);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving category children: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetCategoryByNameAsync(string categoryName)
        {
            try
            {
                var allCategories = await _categoryRepository.GetAllAsync();
                var category = allCategories.FirstOrDefault(c => 
                    c.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase) && c.Active);
                
                if (category == null)
                {
                    return new ApiResponse(false, "Category not found");
                }

                var categoryDto = await MapToCategoryDto(category);
                
                // Get total tours for this category
                categoryDto.TotalTours = await _tourRepository.GetTourCountByCategoryAsync(category.Id);
                
                return new ApiResponse(true, "Category retrieved successfully", categoryDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving category: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetCategoryBySlugAsync(string slug)
        {
            try
            {
                var category = await _categoryRepository.GetBySlugAsync(slug);
                if (category == null)
                {
                    return new ApiResponse(false, "Category not found");
                }

                var categoryDto = await MapToCategoryDto(category);
                
                // Get total tours for this category
                categoryDto.TotalTours = await _tourRepository.GetTourCountByCategoryAsync(category.Id);
                
                return new ApiResponse(true, "Category retrieved successfully", categoryDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving category: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetChildrenTreeAsync(int parentId)
        {
            try
            {
                var children = await _categoryRepository.GetChildrenAsync(parentId);
                var categoryDtos = await MapToCategoryDtos(children);
                
                // Get total tours for each category
                foreach (var categoryDto in categoryDtos)
                {
                    categoryDto.TotalTours = await _tourRepository.GetTourCountByCategoryAsync(categoryDto.Id);
                }
                
                // Build tree structure
                var categoryTree = BuildCategoryTree(categoryDtos, parentId);
                
                return new ApiResponse(true, "Category children tree retrieved successfully", categoryTree);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving category children tree: {ex.Message}");
            }
        }

        private async Task<CategoryDto> MapToCategoryDto(Category category)
        {
            string? parentName = null;
            if (category.ParentId.HasValue)
            {
                var parent = await _categoryRepository.GetByIdAsync(category.ParentId.Value);
                parentName = parent?.CategoryName;
            }

            return new CategoryDto
            {
                Id = category.Id,
                ParentId = category.ParentId,
                ParentName = parentName,
                CategoryName = category.CategoryName,
                Slug = category.Slug ?? "",
                Enable = category.Enable,
                Type = category.Type,
                Priority = category.Priority,
                CreatedTime = category.CreatedTime,
                UpdatedTime = category.UpdatedTime,
                UpdatedBy = category.UpdatedBy,
                Active = category.Active
            };
        }

        private async Task<List<CategoryDto>> MapToCategoryDtos(List<Category> categories)
        {
            var categoryDtos = new List<CategoryDto>();
            
            // Get all unique parent IDs
            var parentIds = categories
                .Where(c => c.ParentId.HasValue)
                .Select(c => c.ParentId.Value)
                .Distinct()
                .ToList();

            // Get all parent categories in one query
            var parentCategories = new Dictionary<int, string>();
            foreach (var parentId in parentIds)
            {
                var parent = await _categoryRepository.GetByIdAsync(parentId);
                if (parent != null)
                {
                    parentCategories[parentId] = parent.CategoryName;
                }
            }

            // Get total children count for each category
            var categoryIds = categories.Select(c => c.Id).ToList();
            var childrenCounts = new Dictionary<int, int>();
            foreach (var categoryId in categoryIds)
            {
                var children = await _categoryRepository.GetChildrenAsync(categoryId);
                childrenCounts[categoryId] = children.Count;
            }

            // Map categories to DTOs
            foreach (var category in categories)
            {
                string? parentName = null;
                if (category.ParentId.HasValue && parentCategories.ContainsKey(category.ParentId.Value))
                {
                    parentName = parentCategories[category.ParentId.Value];
                }

                categoryDtos.Add(new CategoryDto
                {
                    Id = category.Id,
                    ParentId = category.ParentId,
                    ParentName = parentName,
                    CategoryName = category.CategoryName,
                    Slug = category.Slug ?? "",
                    Enable = category.Enable,
                    Type = category.Type,
                    Priority = category.Priority,
                    CreatedTime = category.CreatedTime,
                    UpdatedTime = category.UpdatedTime,
                    UpdatedBy = category.UpdatedBy,
                    Active = category.Active,
                    TotalChild = childrenCounts.ContainsKey(category.Id) ? childrenCounts[category.Id] : 0
                });
            }

            return categoryDtos;
        }

        private List<CategoryTreeDto> BuildCategoryTree(List<CategoryDto> categories, int? parentId)
        {
            return categories
                .Where(c => c.ParentId == parentId)
                .Select(c => new CategoryTreeDto
                {
                    Id = c.Id,
                    ParentId = c.ParentId,
                    CategoryName = c.CategoryName,
                    Slug = c.Slug,
                    Enable = c.Enable,
                    Type = c.Type,
                    Priority = c.Priority,
                    CreatedTime = c.CreatedTime,
                    UpdatedTime = c.UpdatedTime,
                    UpdatedBy = c.UpdatedBy,
                    Active = c.Active,
                    TotalTours = c.TotalTours,
                    Children = BuildCategoryTree(categories, c.Id)
                })
                .ToList();
        }

        #endregion
    }
}
