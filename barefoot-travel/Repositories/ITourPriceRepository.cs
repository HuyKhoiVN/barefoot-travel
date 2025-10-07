using barefoot_travel.DTOs.Tour;
using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public interface ITourPriceRepository
    {
        // TourPrice CRUD operations
        Task<TourPriceResponseDto?> GetByIdAsync(int id);
        Task<List<TourPriceResponseDto>> GetByTourIdAsync(int tourId);
        Task<TourPriceResponseDto> CreateAsync(TourPrice price);
        Task<TourPriceResponseDto> UpdateAsync(TourPrice price);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> TourHasPricesAsync(int tourId);
        Task<decimal> GetMinPriceByTourIdAsync(int tourId);
        Task<decimal> GetMaxPriceByTourIdAsync(int tourId);
    }
}
