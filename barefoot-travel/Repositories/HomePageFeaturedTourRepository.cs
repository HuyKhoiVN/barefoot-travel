using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Repositories
{
    public class HomePageFeaturedTourRepository : IHomePageFeaturedTourRepository
    {
        private readonly SysDbContext _context;

        public HomePageFeaturedTourRepository(SysDbContext context)
        {
            _context = context;
        }

        public async Task<List<HomePageFeaturedTour>> GetAllFeaturedToursAsync()
        {
            return await _context.HomePageFeaturedTours
                .Where(x => x.Active)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }

        public async Task<HomePageFeaturedTour?> GetFeaturedTourByIdAsync(int id)
        {
            return await _context.HomePageFeaturedTours
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<HomePageFeaturedTour?> GetFeaturedTourByTourIdAsync(int tourId)
        {
            return await _context.HomePageFeaturedTours
                .FirstOrDefaultAsync(x => x.TourId == tourId && x.Active);
        }

        public async Task<int> CreateFeaturedTourAsync(HomePageFeaturedTour tour)
        {
            tour.CreatedTime = DateTime.UtcNow;
            _context.HomePageFeaturedTours.Add(tour);
            await _context.SaveChangesAsync();
            return tour.Id;
        }

        public async Task<bool> UpdateFeaturedTourAsync(HomePageFeaturedTour tour)
        {
            tour.UpdatedTime = DateTime.UtcNow;
            _context.HomePageFeaturedTours.Update(tour);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteFeaturedTourAsync(int id)
        {
            var tour = await _context.HomePageFeaturedTours.FindAsync(id);
            if (tour == null) return false;

            tour.Active = false;
            tour.UpdatedTime = DateTime.UtcNow;
            _context.HomePageFeaturedTours.Update(tour);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateDisplayOrdersAsync(List<HomePageFeaturedTour> tours)
        {
            foreach (var tour in tours)
            {
                tour.UpdatedTime = DateTime.UtcNow;
                _context.HomePageFeaturedTours.Update(tour);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<int> GetMaxDisplayOrderAsync()
        {
            var maxOrder = await _context.HomePageFeaturedTours
                .Where(x => x.Active)
                .OrderByDescending(x => x.DisplayOrder)
                .Select(x => x.DisplayOrder)
                .FirstOrDefaultAsync();
            return maxOrder;
        }
    }
}
