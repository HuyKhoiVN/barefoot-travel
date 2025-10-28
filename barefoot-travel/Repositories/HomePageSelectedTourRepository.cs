using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Repositories
{
    public class HomePageSelectedTourRepository : IHomePageSelectedTourRepository
    {
        private readonly SysDbContext _context;

        public HomePageSelectedTourRepository(SysDbContext context)
        {
            _context = context;
        }

        public async Task<List<HomePageSelectedTour>> GetByCategoryIdAsync(int categoryId)
        {
            return await _context.HomePageSelectedTours
                .Where(x => x.CategoryId == categoryId && x.Active)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        public async Task<HomePageSelectedTour?> GetByIdAsync(int id)
        {
            return await _context.HomePageSelectedTours.FindAsync(id);
        }

        public async Task<bool> DeleteByCategoryIdAsync(int categoryId)
        {
            try
            {
                var tours = await _context.HomePageSelectedTours
                    .Where(x => x.CategoryId == categoryId)
                    .ToListAsync();

                _context.HomePageSelectedTours.RemoveRange(tours);
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
                var tour = await GetByIdAsync(id);
                if (tour == null) return false;

                _context.HomePageSelectedTours.Remove(tour);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateAsync(HomePageSelectedTour entity)
        {
            try
            {
                await _context.HomePageSelectedTours.AddAsync(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateRangeAsync(List<HomePageSelectedTour> entities)
        {
            try
            {
                await _context.HomePageSelectedTours.AddRangeAsync(entities);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(HomePageSelectedTour entity)
        {
            try
            {
                _context.HomePageSelectedTours.Update(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(int categoryId, int tourId)
        {
            return await _context.HomePageSelectedTours
                .AnyAsync(x => x.CategoryId == categoryId && x.TourId == tourId && x.Active);
        }
    }
}
