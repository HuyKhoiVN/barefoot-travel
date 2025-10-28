using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public interface IHomePageSelectedTourRepository
    {
        Task<List<HomePageSelectedTour>> GetByCategoryIdAsync(int categoryId);
        Task<HomePageSelectedTour?> GetByIdAsync(int id);
        Task<bool> DeleteByCategoryIdAsync(int categoryId);
        Task<bool> DeleteAsync(int id);
        Task<bool> CreateAsync(HomePageSelectedTour entity);
        Task<bool> CreateRangeAsync(List<HomePageSelectedTour> entities);
        Task<bool> UpdateAsync(HomePageSelectedTour entity);
        Task<bool> ExistsAsync(int categoryId, int tourId);
    }
}
