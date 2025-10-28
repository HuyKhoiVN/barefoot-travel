using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Repositories
{
    public class HomePageSectionTourRepository : IHomePageSectionTourRepository
    {
        private readonly SysDbContext _context;

        public HomePageSectionTourRepository(SysDbContext context)
        {
            _context = context;
        }

        public async Task<List<HomePageSectionTour>> GetBySectionIdAsync(int sectionId)
        {
            return await (from st in _context.HomePageSectionTours
                         where st.SectionId == sectionId && st.Active
                         orderby st.DisplayOrder
                         select st).ToListAsync();
        }

        public async Task<HomePageSectionTour?> GetByIdAsync(int id)
        {
            return await (from st in _context.HomePageSectionTours
                         where st.Id == id
                         select st).FirstOrDefaultAsync();
        }

        public async Task<bool> DeleteBySectionIdAsync(int sectionId)
        {
            try
            {
                var tours = await _context.HomePageSectionTours
                    .Where(x => x.SectionId == sectionId)
                    .ToListAsync();

                _context.HomePageSectionTours.RemoveRange(tours);
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

                _context.HomePageSectionTours.Remove(tour);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateAsync(HomePageSectionTour entity)
        {
            try
            {
                entity.CreatedTime = DateTime.UtcNow;
                entity.Active = true;
                
                await _context.HomePageSectionTours.AddAsync(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CreateRangeAsync(List<HomePageSectionTour> entities)
        {
            try
            {
                var now = DateTime.UtcNow;
                foreach (var entity in entities)
                {
                    entity.CreatedTime = now;
                    entity.Active = true;
                }
                
                await _context.HomePageSectionTours.AddRangeAsync(entities);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(HomePageSectionTour entity)
        {
            try
            {
                entity.UpdatedTime = DateTime.UtcNow;
                
                _context.HomePageSectionTours.Update(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExistsAsync(int sectionId, int tourId)
        {
            return await _context.HomePageSectionTours
                .AnyAsync(x => x.SectionId == sectionId && x.TourId == tourId && x.Active);
        }
    }
}

