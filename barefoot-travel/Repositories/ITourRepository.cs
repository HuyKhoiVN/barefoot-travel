using barefoot_travel.Models;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Tour;
using HomepageTourDto = barefoot_travel.DTOs.HomepageTourDto;

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
        Task<Tour?> GetBySlugAsync(string slug);
        Task<bool> SlugExistsAsync(string slug, int? excludeId = null);
        Task<List<string>> GetAllSlugsAsync();

        // Tour with related data - DTO methods with joins
        Task<TourDetailDto?> GetTourDetailByIdAsync(int id);
        Task<TourDetailDto?> GetTourDetailBySlugAsync(string slug);
        Task<List<TourDto>> GetToursWithBasicInfoAsync();
        Task<PagedResult<TourDto>> GetToursPagedWithBasicInfoAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", List<int>? categoryIds = null, string? search = null, bool? active = null);
        Task<List<TourDto>> GetToursByCategoryAsync(int categoryId);

        // Optimized bulk operations
        Task<List<TourDetailDto>> GetToursWithRelatedDataAsync(List<int> tourIds);
        Task<List<TourDto>> GetToursWithBannerImageAsync(int? categoryId = null, int? limit = null);

        // Tour status operations
        Task<bool> UpdateStatusAsync(int id, bool active, string updatedBy);
        Task<bool> HasActiveBookingsAsync(int tourId);

        // Tour Image Operations (Basic)
        Task<bool> TourHasImagesAsync(int tourId);
        Task<int> GetImageCountByTourIdAsync(int tourId);

        // Tour Category Operations (Basic)
        Task<bool> TourHasCategoriesAsync(int tourId);
        Task<bool> CategoryLinkExistsAsync(int tourId, int categoryId);

        // Tour Price Operations (Basic)
        Task<bool> TourHasPricesAsync(int tourId);
        Task<decimal> GetMinPriceByTourIdAsync(int tourId);
        Task<decimal> GetMaxPriceByTourIdAsync(int tourId);

        // Tour Policy Operations (Basic)
        Task<bool> TourHasPoliciesAsync(int tourId);
        Task<bool> PolicyLinkExistsAsync(int tourId, int policyId);

        // Marketing Tag Operations (Basic)
        Task<bool> TourHasMarketingTagsAsync(int tourId);

        // Validation methods
        Task<bool> TourExistsAsync(int id);
        Task<bool> CategoryExistsAsync(int id);
        Task<bool> PriceTypeExistsAsync(int id);
        Task<bool> PolicyExistsAsync(int id);
        Task<bool> IsMarketingCategoryAsync(int categoryId);

        // Homepage methods
        Task<List<HomepageTourDto>> GetToursByCategoryForHomepageAsync(int categoryId, int maxItems);
        Task<List<HomepageTourDto>> GetToursByIdsAsync(List<int> tourIds, int maxItems);
        Task<int> GetTourCountByCategoryAsync(int categoryId);

        // Tour Status Approval Methods
        Task<bool> ChangeStatusAsync(int tourId, string newStatus, string updatedBy, string? reason = null);
        Task<PagedResult<TourWithStatusDto>> GetToursPagedByStatusAsync(string? status, int page, int pageSize, string? sortBy = null, string? sortOrder = "asc");
        Task<List<TourStatusHistoryDto>> GetStatusHistoryAsync(int tourId);
        Task<BatchOperationResultDto> BatchChangeStatusAsync(List<int> tourIds, string newStatus, string updatedBy, string? reason = null);
        Task<BatchOperationResultDto> BatchDeleteAsync(List<int> tourIds, string updatedBy);
        Task<bool> CanDeleteAsync(int tourId);
        Task<string?> GetCurrentStatusAsync(int tourId);
        Task<bool> SoftDeleteAsync(int tourId, string updatedBy);
    }
}
