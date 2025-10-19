using barefoot_travel.DTOs.Tour;
using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public interface ITourCategoryRepository
    {
        // TourCategory CRUD operations
        Task<TourCategoryResponseDto?> GetByIdAsync(int id);
        Task<List<TourCategoryResponseDto>> GetByTourIdAsync(int tourId);
        Task<TourCategoryResponseDto> CreateAsync(TourCategory category);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByTourIdAsync(int tourId);
        Task<bool> ExistsAsync(int id);
        Task<bool> LinkExistsAsync(int tourId, int categoryId);
        Task<List<MarketingTagDto>> GetMarketingTagsByTourIdAsync(int tourId);
        Task<MarketingTagDto> CreateMarketingTagAsync(TourCategory category);
        Task<bool> DeleteMarketingTagAsync(int tourId, int categoryId);
    }
}
