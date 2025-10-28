using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public interface IHomePageSectionTourRepository
    {
        Task<List<HomePageSectionTour>> GetBySectionIdAsync(int sectionId);
        Task<HomePageSectionTour?> GetByIdAsync(int id);
        Task<bool> DeleteBySectionIdAsync(int sectionId);
        Task<bool> DeleteAsync(int id);
        Task<bool> CreateAsync(HomePageSectionTour entity);
        Task<bool> CreateRangeAsync(List<HomePageSectionTour> entities);
        Task<bool> UpdateAsync(HomePageSectionTour entity);
        Task<bool> ExistsAsync(int sectionId, int tourId);
    }
}
