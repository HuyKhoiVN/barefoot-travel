using barefoot_travel.DTOs.Tour;
using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Repositories
{
    public class TourCategoryRepository : ITourCategoryRepository
    {
        private readonly SysDbContext _context;

        public TourCategoryRepository(SysDbContext context)
        {
            _context = context;
        }

        public async Task<TourCategoryResponseDto?> GetByIdAsync(int id)
        {
            return await (from tc in _context.TourCategories
                         join c in _context.Categories on tc.CategoryId equals c.Id
                         where tc.Id == id && tc.Active && c.Active
                         select new TourCategoryResponseDto
                         {
                             Id = tc.Id,
                             TourId = tc.TourId,
                             CategoryId = tc.CategoryId,
                             CategoryName = c.CategoryName,
                             CreatedTime = tc.CreatedTime
                         }).FirstOrDefaultAsync();
        }

        public async Task<List<TourCategoryResponseDto>> GetByTourIdAsync(int tourId)
        {
            return await (from tc in _context.TourCategories
                         join c in _context.Categories on tc.CategoryId equals c.Id
                         where tc.TourId == tourId && tc.Active && c.Active
                         orderby tc.CreatedTime
                         select new TourCategoryResponseDto
                         {
                             Id = tc.Id,
                             TourId = tc.TourId,
                             CategoryId = tc.CategoryId,
                             CategoryName = c.CategoryName,
                             CreatedTime = tc.CreatedTime
                         }).ToListAsync();
        }

        public async Task<TourCategoryResponseDto> CreateAsync(TourCategory category)
        {
            category.CreatedTime = DateTime.UtcNow;
            category.Active = true;
            await _context.TourCategories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Get category name for response
            var categoryName = await _context.Categories
                .Where(c => c.Id == category.CategoryId)
                .Select(c => c.CategoryName)
                .FirstOrDefaultAsync() ?? "";

            return new TourCategoryResponseDto
            {
                Id = category.Id,
                TourId = category.TourId,
                CategoryId = category.CategoryId,
                CategoryName = categoryName,
                CreatedTime = category.CreatedTime
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var category = await _context.TourCategories
                .Where(tc => tc.Id == id && tc.Active)
                .FirstOrDefaultAsync();

            if (category == null) return false;

            category.Active = false;
            category.UpdatedTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByTourIdAsync(int tourId)
        {
            var categories = await _context.TourCategories
                .Where(tc => tc.TourId == tourId && tc.Active)
                .ToListAsync();

            if (!categories.Any()) return false;

            foreach (var category in categories)
            {
                category.Active = false;
                category.UpdatedTime = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.TourCategories
                .Where(tc => tc.Id == id && tc.Active)
                .AnyAsync();
        }

        public async Task<bool> LinkExistsAsync(int tourId, int categoryId)
        {
            return await _context.TourCategories
                .Where(tc => tc.TourId == tourId && tc.CategoryId == categoryId && tc.Active)
                .AnyAsync();
        }

        public async Task<List<MarketingTagDto>> GetMarketingTagsByTourIdAsync(int tourId)
        {
            return await (from tc in _context.TourCategories
                         join c in _context.Categories on tc.CategoryId equals c.Id
                         where tc.TourId == tourId && tc.Active && c.Active && c.Type == "Marketing"
                         orderby tc.CreatedTime
                         select new MarketingTagDto
                         {
                             Id = tc.Id,
                             TourId = tc.TourId,
                             CategoryId = tc.CategoryId,
                             CategoryName = c.CategoryName,
                             CreatedTime = tc.CreatedTime
                         }).ToListAsync();
        }

        public async Task<MarketingTagDto> CreateMarketingTagAsync(TourCategory category)
        {
            category.CreatedTime = DateTime.UtcNow;
            category.Active = true;
            await _context.TourCategories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Get category name for response
            var categoryName = await _context.Categories
                .Where(c => c.Id == category.CategoryId)
                .Select(c => c.CategoryName)
                .FirstOrDefaultAsync() ?? "";

            return new MarketingTagDto
            {
                Id = category.Id,
                TourId = category.TourId,
                CategoryId = category.CategoryId,
                CategoryName = categoryName,
                CreatedTime = category.CreatedTime
            };
        }

        public async Task<bool> DeleteMarketingTagAsync(int tourId, int categoryId)
        {
            var tourCategory = await _context.TourCategories
                .Where(tc => tc.TourId == tourId && tc.CategoryId == categoryId && tc.Active)
                .FirstOrDefaultAsync();

            if (tourCategory == null) return false;

            tourCategory.Active = false;
            tourCategory.UpdatedTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
