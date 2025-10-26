using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public interface IHomePageFeaturedTourRepository
    {
        Task<List<HomePageFeaturedTour>> GetAllFeaturedToursAsync();
        Task<HomePageFeaturedTour?> GetFeaturedTourByIdAsync(int id);
        Task<HomePageFeaturedTour?> GetFeaturedTourByTourIdAsync(int tourId);
        Task<int> CreateFeaturedTourAsync(HomePageFeaturedTour tour);
        Task<bool> UpdateFeaturedTourAsync(HomePageFeaturedTour tour);
        Task<bool> DeleteFeaturedTourAsync(int id);
        Task<bool> UpdateDisplayOrdersAsync(List<HomePageFeaturedTour> tours);
        Task<int> GetMaxDisplayOrderAsync();
    }
}
