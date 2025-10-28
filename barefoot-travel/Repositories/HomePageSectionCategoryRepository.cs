using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Repositories
{
    public class HomePageSectionCategoryRepository : IHomePageSectionCategoryRepository
    {
        private readonly SysDbContext _context;

        public HomePageSectionCategoryRepository(SysDbContext context)
        {
            _context = context;
        }

        public async Task<List<HomePageSectionCategory>> GetBySectionIdAsync(int sectionId)
        {
            return await (from sc in _context.HomePageSectionCategories
                         where sc.SectionId == sectionId && sc.Active
                         orderby sc.DisplayOrder
                         select sc).ToListAsync();
        }

        public async Task<HomePageSectionCategory?> GetByIdAsync(int id)
        {
            return await (from sc in _context.HomePageSectionCategories
                         where sc.Id == id
                         select sc).FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteBySectionIdAsync(int sectionId)
        {
            try
            {
                var items = await _context.HomePageSectionCategories
                    .Where(x => x.SectionId == sectionId)
                    .ToListAsync();

                _context.HomePageSectionCategories.RemoveRange(items);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var item = await GetByIdAsync(id);
                if (item == null) return false;

                _context.HomePageSectionCategories.Remove(item);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateAsync(HomePageSectionCategory entity)
        {
            try
            {
                await _context.HomePageSectionCategories.AddAsync(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateRangeAsync(List<HomePageSectionCategory> entities)
        {
            try
            {
                await _context.HomePageSectionCategories.AddRangeAsync(entities);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(HomePageSectionCategory entity)
        {
            try
            {
                _context.HomePageSectionCategories.Update(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(int sectionId, int categoryId)
        {
            return await _context.HomePageSectionCategories
                .AnyAsync(x => x.SectionId == sectionId && x.CategoryId == categoryId && x.Active);
        }
    }
}
