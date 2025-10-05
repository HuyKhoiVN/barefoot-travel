using barefoot_travel.Models;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Tour;

namespace barefoot_travel.Repositories
{
    public interface ITourRepository
    {
        // Tour CRUD operations
        Task<Tour?> GetByIdAsync(int id);
        Task<List<Tour>> GetAllAsync();
        Task<PagedResult<Tour>> GetPagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", int? categoryId = null, bool? active = null);
        Task<Tour> CreateAsync(Tour tour);
        Task<Tour> UpdateAsync(Tour tour);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> TitleExistsAsync(string title, int? excludeId = null);

        // Tour with related data - DTO methods with joins
        Task<TourDetailDto?> GetTourDetailByIdAsync(int id);
        Task<List<TourDto>> GetToursWithBasicInfoAsync();
        Task<PagedResult<TourDto>> GetToursPagedWithBasicInfoAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", int? categoryId = null, bool? active = null);
        Task<List<TourDto>> GetToursByCategoryAsync(int categoryId);

        // Optimized bulk operations
        Task<List<TourDetailDto>> GetToursWithRelatedDataAsync(List<int> tourIds);
        Task<List<TourDto>> GetToursWithBannerImageAsync(int? categoryId = null, int? limit = null);

        // Tour status operations
        Task<bool> UpdateStatusAsync(int id, bool active, string updatedBy);
        Task<bool> HasActiveBookingsAsync(int tourId);

        // TourImage operations with DTOs
        Task<TourImageResponseDto?> GetImageByIdAsync(int id);
        Task<List<TourImageResponseDto>> GetImagesByTourIdAsync(int tourId);
        Task<TourImageResponseDto> CreateImageAsync(TourImage image);
        Task<TourImageResponseDto> UpdateImageAsync(TourImage image);
        Task<bool> DeleteImageAsync(int id);

        // TourCategory operations with DTOs
        Task<TourCategoryResponseDto?> GetCategoryByIdAsync(int id);
        Task<List<TourCategoryResponseDto>> GetCategoriesByTourIdAsync(int tourId);
        Task<TourCategoryResponseDto> CreateCategoryAsync(TourCategory category);
        Task<bool> DeleteCategoryAsync(int id);
        Task<bool> CategoryLinkExistsAsync(int tourId, int categoryId);

        // TourPrice operations with DTOs
        Task<TourPriceResponseDto?> GetPriceByIdAsync(int id);
        Task<List<TourPriceResponseDto>> GetPricesByTourIdAsync(int tourId);
        Task<TourPriceResponseDto> CreatePriceAsync(TourPrice price);
        Task<TourPriceResponseDto> UpdatePriceAsync(TourPrice price);
        Task<bool> DeletePriceAsync(int id);

        // TourPolicy operations with DTOs
        Task<TourPolicyResponseDto?> GetPolicyByIdAsync(int id);
        Task<List<TourPolicyResponseDto>> GetPoliciesByTourIdAsync(int tourId);
        Task<TourPolicyResponseDto> CreatePolicyAsync(TourPolicy policy);
        Task<bool> DeletePolicyAsync(int id);
        Task<bool> PolicyLinkExistsAsync(int tourId, int policyId);

        // Marketing tag operations with DTOs
        Task<List<MarketingTagDto>> GetMarketingTagsByTourIdAsync(int tourId);
        Task<MarketingTagDto> CreateMarketingTagAsync(TourCategory category);
        Task<bool> DeleteMarketingTagAsync(int tourId, int categoryId);

        // Validation methods
        Task<bool> TourExistsAsync(int id);
        Task<bool> CategoryExistsAsync(int id);
        Task<bool> PriceTypeExistsAsync(int id);
        Task<bool> PolicyExistsAsync(int id);
        Task<bool> IsMarketingCategoryAsync(int categoryId);
    }
}
