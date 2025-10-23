using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Category;
using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly SysDbContext _context;

        public CategoryRepository(SysDbContext context)
        {
            _context = context;
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories
                .Where(c => c.Id == id && c.Active)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _context.Categories
                .Where(c => c.Active)
                .OrderBy(c => c.Type).ThenBy(c => c.Priority).ThenBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<PagedResult<Category>> GetPagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", string? categoryName = null, string? type = null, List<int>? categoryIds = null, bool? active = null)
        {
            var query = _context.Categories.AsQueryable();

            // Filter by active status
            if (active.HasValue)
            {
                query = query.Where(c => c.Active == active.Value);
            }
            else
            {
                query = query.Where(c => c.Active);
            }

            // Filter by category name (search)
            if (!string.IsNullOrEmpty(categoryName))
            {
                query = query.Where(c => c.CategoryName.Contains(categoryName));
            }

            // Filter by type
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(c => c.Type == type);
            }

            // Filter by category IDs (including descendants)
            if (categoryIds != null && categoryIds.Any())
            {
                var allDescendantIds = new List<int>();
                foreach (var categoryId in categoryIds)
                {
                    allDescendantIds.Add(categoryId);
                    var descendants = await GetDescendantCategoryIds(categoryId);
                    allDescendantIds.AddRange(descendants);
                }
                
                query = query.Where(c => allDescendantIds.Contains(c.ParentId ?? 0));
            }

            // Apply sorting
            var sortedQuery = sortBy?.ToLower() switch
            {
                "name" => sortOrder == "desc"
                    ? query.OrderByDescending(c => c.CategoryName)
                    : query.OrderBy(c => c.CategoryName),
                "categoryname" => sortOrder == "desc"
                    ? query.OrderByDescending(c => c.CategoryName)
                    : query.OrderBy(c => c.CategoryName),
                "type" => sortOrder == "desc"
                    ? query.OrderByDescending(c => c.Type)
                    : query.OrderBy(c => c.Type),
                "parentname" => sortOrder == "desc"
                    ? query.OrderByDescending(c => c.ParentId)
                    : query.OrderBy(c => c.ParentId),
                "enable" => sortOrder == "desc"
                    ? query.OrderByDescending(c => c.Enable)
                    : query.OrderBy(c => c.Enable),
                "priority" => sortOrder == "desc"
                    ? query.OrderByDescending(c => c.Priority)
                    : query.OrderBy(c => c.Priority),
                "createdtime" => sortOrder == "desc"
                    ? query.OrderByDescending(c => c.CreatedTime)
                    : query.OrderBy(c => c.CreatedTime),
                _ => query.OrderBy(c => c.CategoryName)
            };

            var totalItems = await sortedQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var items = await sortedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Category>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }

        public async Task<Category> CreateAsync(Category category)
        {
            category.CreatedTime = DateTime.UtcNow;
            category.Active = true;
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<Category> UpdateAsync(Category category)
        {
            category.UpdatedTime = DateTime.UtcNow;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.Categories
                .Where(c => c.Id == id && c.Active)
                .FirstOrDefaultAsync();

            if (category == null) return false;

            category.Active = false;
            category.UpdatedTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories
                .Where(c => c.Id == id && c.Active)
                .AnyAsync();
        }

        public async Task<bool> NameExistsAsync(string categoryName, int? excludeId = null)
        {
            var query = _context.Categories.Where(c => c.CategoryName == categoryName && c.Active);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> HasChildrenAsync(int id)
        {
            return await _context.Categories
                .Where(c => c.ParentId == id && c.Active)
                .AnyAsync();
        }

        public async Task<List<Category>> GetByTypeAsync(string type)
        {
            return await _context.Categories
                .Where(c => c.Type == type && c.Active).OrderBy(c => c.Priority)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<List<Category>> GetByParentIdAsync(int? parentId)
        {
            return await _context.Categories
                .Where(c => c.ParentId == parentId && c.Active).OrderBy(c => c.Priority)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<bool> UpdateStatusAsync(int id, bool active, string updatedBy)
        {
            var category = await _context.Categories
                .Where(c => c.Id == id).OrderBy(c => c.Priority)
                .FirstOrDefaultAsync();

            if (category == null) return false;

            category.Active = active;
            category.UpdatedTime = DateTime.UtcNow;
            category.UpdatedBy = updatedBy;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<string>> GetAllType()
        {
            return await _context.Categories
                .Where(c => c.Active).OrderBy(c => c.Priority)
                .Select(c => c.Type)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<int>> GetDescendantCategoryIds(int parentId)
        {
            var descendantIds = new List<int>();
            var directChildren = await _context.Categories
                .Where(c => c.ParentId == parentId && c.Active)
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var childId in directChildren)
            {
                descendantIds.Add(childId);
                var grandChildren = await GetDescendantCategoryIds(childId);
                descendantIds.AddRange(grandChildren);
            }

            return descendantIds;
        }

        public async Task<PagedResult<Category>> GetTreePagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", string? categoryName = null, string? type = null, List<int>? categoryIds = null, bool? active = null)
        {
            var query = from c in _context.Categories
                       select new Category
                       {
                           Id = c.Id,
                           ParentId = c.ParentId,
                           CategoryName = c.CategoryName,
                           Enable = c.Enable,
                           Type = c.Type,
                           Priority = c.Priority,
                           CreatedTime = c.CreatedTime,
                           UpdatedTime = c.UpdatedTime,
                           UpdatedBy = c.UpdatedBy,
                           Active = c.Active
                       };

            // Filter by active status
            if (active.HasValue)
            {
                query = query.Where(c => c.Active == active.Value);
            }
            else
            {
                query = query.Where(c => c.Active);
            }

            // Filter by category name (search)
            if (!string.IsNullOrEmpty(categoryName))
            {
                query = query.Where(c => c.CategoryName.Contains(categoryName));
            }

            // Filter by type
            if (!string.IsNullOrEmpty(type))
            {
                query = query.Where(c => c.Type == type);
            }

            // Filter by category IDs - only show categories that are direct children of selected categories
            if (categoryIds != null && categoryIds.Any())
            {
                query = query.Where(c => categoryIds.Contains(c.ParentId ?? 0));
            }
            else
            {
                // If no categoryIds filter, only show root level categories (ParentId is null)
                query = query.Where(c => c.ParentId == null);
            }

            // Apply sorting
            var sortedQuery = sortBy?.ToLower() switch
            {
                "name" => sortOrder == "desc"
                    ? query.OrderByDescending(c => c.CategoryName)
                    : query.OrderBy(c => c.CategoryName),
                "categoryname" => sortOrder == "desc"
                    ? query.OrderByDescending(c => c.CategoryName)
                    : query.OrderBy(c => c.CategoryName),
                "type" => sortOrder == "desc"
                    ? query.OrderByDescending(c => c.Type)
                    : query.OrderBy(c => c.Type),
                "parentname" => sortOrder == "desc"
                    ? query.OrderByDescending(c => c.ParentId)
                    : query.OrderBy(c => c.ParentId),
                "enable" => sortOrder == "desc"
                    ? query.OrderByDescending(c => c.Enable)
                    : query.OrderBy(c => c.Enable),
                "priority" => sortOrder == "desc"
                    ? query.OrderByDescending(c => c.Priority)
                    : query.OrderBy(c => c.Priority),
                "createdtime" => sortOrder == "desc"
                    ? query.OrderByDescending(c => c.CreatedTime)
                    : query.OrderBy(c => c.CreatedTime),
                _ => query.OrderBy(c => c.CategoryName)
            };

            var totalItems = await sortedQuery.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var items = await sortedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Category>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }
         
        public async Task<List<Category>> GetChildrenAsync(int parentId)
        {
            return await _context.Categories
                .Where(c => c.ParentId == parentId && c.Active)
                .OrderBy(c => c.Priority)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();
        }
    }
}
