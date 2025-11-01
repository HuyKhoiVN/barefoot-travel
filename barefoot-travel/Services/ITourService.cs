using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Tour;

namespace barefoot_travel.Services
{
    public interface ITourService
    {
        // Tour CRUD operations
        Task<ApiResponse> GetTourByIdAsync(int id);
        Task<ApiResponse> GetTourBySlugAsync(string slug);
        Task<ApiResponse> GetAllToursAsync();
        Task<List<DTOs.HomepageTourDto>> GetToursByCategoryForHomepageAsync(int categoryId, int maxItems);
        Task<PagedResult<TourDto>> GetToursPagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", List<int>? categoryIds = null, string? search = null, bool? active = null);
        Task<PagedResult<TourDto>> GetToursByCategoryPagedAsync(int categoryId, int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", string? search = null);
        Task<ApiResponse> SearchToursAsync(string query, int limit = 10);
        Task<ApiResponse> CreateTourAsync(CreateTourDto dto, string adminUsername);
        Task<ApiResponse> UpdateTourAsync(int id, UpdateTourDto dto, string adminUsername);
        Task<ApiResponse> DeleteTourAsync(int id, string adminUsername);

        // Tour status operations
        Task<ApiResponse> UpdateTourStatusAsync(int id, bool active, string adminUsername);

        // Tour itinerary operations
        Task<ApiResponse> UpdateTourItineraryAsync(int id, string itineraryJson, string adminUsername);

        // Marketing tag operations
        Task<ApiResponse> AddMarketingTagAsync(int tourId, int categoryId, string adminUsername);
        Task<ApiResponse> RemoveMarketingTagAsync(int tourId, int categoryId, string adminUsername);
        Task<ApiResponse> GetMarketingTagsAsync(int tourId);

        // TourImage operations
        Task<ApiResponse> CreateTourImageAsync(CreateTourImageDto dto, string adminUsername);
        Task<ApiResponse> UpdateTourImageAsync(int id, UpdateTourImageDto dto, string adminUsername);
        Task<ApiResponse> DeleteTourImageAsync(int id, string adminUsername);
        Task<ApiResponse> GetTourImagesAsync(int tourId);
        Task<ApiResponse> UploadTourImageAsync(CreateTourImageDto dto, IFormFile file, string adminUsername);
        Task<ApiResponse> SetTourImageAsBannerAsync(int imageId, string adminUsername);
        Task<ApiResponse> RemoveTourImageBannerAsync(int imageId, string adminUsername);

        // TourCategory operations
        Task<ApiResponse> CreateTourCategoryAsync(CreateTourCategoryDto dto, string adminUsername);
        Task<ApiResponse> DeleteTourCategoryAsync(int id, string adminUsername);
        Task<ApiResponse> GetTourCategoriesAsync(int tourId);

        // TourPrice operations
        Task<ApiResponse> CreateTourPriceAsync(CreateTourPriceDto dto, string adminUsername);
        Task<ApiResponse> UpdateTourPriceAsync(int id, UpdateTourPriceDto dto, string adminUsername);
        Task<ApiResponse> DeleteTourPriceAsync(int id, string adminUsername);
        Task<ApiResponse> GetTourPricesAsync(int tourId);

        // TourPolicy operations
        Task<ApiResponse> CreateTourPolicyAsync(CreateTourPolicyDto dto, string adminUsername);
        Task<ApiResponse> DeleteTourPolicyAsync(int id, string adminUsername);
        Task<ApiResponse> GetTourPoliciesAsync(int tourId);

        // Tour Status Approval Methods
        Task<ApiResponse> ChangeStatusAsync(int tourId, string newStatus, string updatedBy, string? reason = null);
        Task<PagedResult<TourWithStatusDto>> GetToursPagedByStatusAsync(string? status, int page, int pageSize, string? sortBy = null, string? sortOrder = "asc");
        Task<ApiResponse> GetStatusHistoryAsync(int tourId);
        Task<ApiResponse> BatchChangeStatusAsync(List<int> tourIds, string newStatus, string updatedBy, string? reason = null);
        Task<ApiResponse> BatchDeleteToursAsync(List<int> tourIds, string updatedBy);
    }
}
