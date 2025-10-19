using barefoot_travel.DTOs.Tour;
using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public interface ITourImageRepository
    {
        // TourImage CRUD operations
        Task<TourImageResponseDto?> GetByIdAsync(int id);
        Task<List<TourImageResponseDto>> GetByTourIdAsync(int tourId);
        Task<TourImageResponseDto> CreateAsync(TourImage image);
        Task<TourImageResponseDto> UpdateAsync(TourImage image);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> TourHasImagesAsync(int tourId);
        Task<int> GetImageCountByTourIdAsync(int tourId);
        Task<bool> UpdateBanner(int id);
        Task<bool> RemoveBanner(int id);
    }
}
