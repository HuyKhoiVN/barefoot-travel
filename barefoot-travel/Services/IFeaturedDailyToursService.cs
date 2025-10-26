using barefoot_travel.DTOs.Category;

namespace barefoot_travel.Services
{
    public interface IFeaturedDailyToursService
    {
        // Featured Tours
        Task<FeaturedToursConfigDto> GetFeaturedToursAsync();
        Task ConfigureFeaturedTourAsync(int tourId, ConfigureFeaturedTourDto dto, string userId);
        Task UpdateFeaturedTourAsync(int id, ConfigureFeaturedTourDto dto, string userId);
        Task RemoveFeaturedTourAsync(int id, string userId);
        Task ReorderFeaturedToursAsync(List<ReorderTourDto> orders, string userId);

        // Daily Tours
        Task<DailyToursConfigDto> GetDailyToursAsync();
        Task ConfigureDailyTourAsync(int categoryId, ConfigureDailyTourDto dto, string userId);
        Task RemoveDailyTourAsync(int categoryId, string userId);
        Task ReorderDailyToursAsync(List<ReorderTourDto> orders, string userId);
    }
}
