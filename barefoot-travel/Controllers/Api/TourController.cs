using barefoot_travel.Common;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Tour;
using barefoot_travel.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace barefoot_travel.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TourController : ControllerBase
    {
        private readonly ITourService _tourService;
        private readonly ILogger<TourController> _logger;

        public TourController(ITourService tourService, ILogger<TourController> logger)
        {
            _tourService = tourService;
            _logger = logger;
        }

        #region Tour CRUD Operations

        /// <summary>
        /// Get a specific tour by ID
        /// </summary>
        /// <param name="id">Tour ID</param>
        /// <returns>Tour details with related data</returns>
        [HttpGet("{id}")]
        public async Task<ApiResponse> GetTour(int id)
        {
            _logger.LogInformation("Getting tour with ID: {TourId}", id);
            return await _tourService.GetTourByIdAsync(id);
        }

        /// <summary>
        /// Get all tours
        /// </summary>
        /// <returns>List of all tours</returns>
        [HttpGet]
        public async Task<ApiResponse> GetAllTours()
        {
            _logger.LogInformation("Getting all tours");
            return await _tourService.GetAllToursAsync();
        }

        /// <summary>
        /// Get tours by category ID for homepage preview
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <param name="maxItems">Maximum items to return (default: 20)</param>
        /// <returns>List of tours for homepage display</returns>
        [HttpGet("by-category/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetToursByCategory(int categoryId, [FromQuery] int maxItems = 20)
        {
            try
            {
                var tours = await _tourService.GetToursByCategoryForHomepageAsync(categoryId, maxItems);
                return Ok(new ApiResponse(true, "Tours retrieved successfully", tours));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tours by category");
                return BadRequest(new ApiResponse(false, "Failed to get tours"));
            }
        }

        /// <summary>
        /// Get paginated list of tours with filtering and sorting
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Items per page (default: 10)</param>
        /// <param name="sortBy">Sort field (title, pricePerPerson, createdTime)</param>
        /// <param name="sortOrder">Sort direction (asc, desc)</param>
        /// <param name="categoryIds">Filter by category IDs (comma-separated)</param>
        /// <param name="search">Search by title</param>
        /// <param name="active">Filter by active status</param>
        /// <returns>Paginated list of tours</returns>
        [HttpGet("paged")]
        public async Task<PagedResult<TourDto>> GetToursPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? sortBy = null,
            [FromQuery] string sortOrder = "asc",
            [FromQuery] string? categoryIds = null,
            [FromQuery] string? search = null,
            [FromQuery] bool? active = null)
        {
            _logger.LogInformation("Getting paged tours - Page: {Page}, PageSize: {PageSize}", page, pageSize);
            
            // Parse categoryIds from comma-separated string
            List<int>? categoryIdList = null;
            if (!string.IsNullOrEmpty(categoryIds))
            {
                categoryIdList = categoryIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList();
            }
            
            return await _tourService.GetToursPagedAsync(page, pageSize, sortBy, sortOrder, categoryIdList, search, active);
        }

        /// <summary>
        /// Create a new tour
        /// </summary>
        /// <param name="dto">Tour creation data</param>
        /// <returns>Created tour details</returns>
        [HttpPost]
        public async Task<ApiResponse> CreateTour([FromBody] CreateTourDto dto)
        {
            _logger.LogInformation("Creating tour: {Title}", dto.Title);
            var adminUsername = GetAdminUsername();
            return await _tourService.CreateTourAsync(dto, adminUsername);
        }

        /// <summary>
        /// Update an existing tour
        /// </summary>
        /// <param name="id">Tour ID</param>
        /// <param name="dto">Tour update data</param>
        /// <returns>Updated tour details</returns>
        [HttpPut("{id}")]
        public async Task<ApiResponse> UpdateTour(int id, [FromBody] UpdateTourDto dto)
        {
            _logger.LogInformation("Updating tour with ID: {TourId}", id);
            var adminUsername = GetAdminUsername();
            return await _tourService.UpdateTourAsync(id, dto, adminUsername);
        }

        /// <summary>
        /// Delete a tour (soft delete)
        /// </summary>
        /// <param name="id">Tour ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}")]
        public async Task<ApiResponse> DeleteTour(int id)
        {
            _logger.LogInformation("Deleting tour with ID: {TourId}", id);
            var adminUsername = GetAdminUsername();
            return await _tourService.DeleteTourAsync(id, adminUsername);
        }

        #endregion

        #region Tour Status Operations

        /// <summary>
        /// Update tour status (enable/disable)
        /// </summary>
        /// <param name="id">Tour ID</param>
        /// <param name="dto">Status update data</param>
        /// <returns>Updated status</returns>
        [HttpPut("{id}/status")]
        public async Task<ApiResponse> UpdateTourStatus(int id, [FromBody] TourStatusDto dto)
        {
            _logger.LogInformation("Updating tour status for ID: {TourId}, Active: {Active}", id, dto.Active);
            var adminUsername = GetAdminUsername();
            return await _tourService.UpdateTourStatusAsync(id, dto.Active, adminUsername);
        }

        #endregion

        #region Tour Itinerary Operations

        /// <summary>
        /// Update tour itinerary
        /// </summary>
        /// <param name="id">Tour ID</param>
        /// <param name="dto">Itinerary data</param>
        /// <returns>Updated itinerary</returns>
        [HttpPut("{id}/itinerary")]
        public async Task<ApiResponse> UpdateTourItinerary(int id, [FromBody] TourItineraryDto dto)
        {
            _logger.LogInformation("Updating tour itinerary for ID: {TourId}", id);
            var adminUsername = GetAdminUsername();
            return await _tourService.UpdateTourItineraryAsync(id, dto.ItineraryJson, adminUsername);
        }

        #endregion

        #region Marketing Tag Operations

        /// <summary>
        /// Add marketing tag to tour
        /// </summary>
        /// <param name="id">Tour ID</param>
        /// <param name="dto">Marketing tag data</param>
        /// <returns>Added marketing tag</returns>
        [HttpPost("{id}/marketing-tag")]
        public async Task<ApiResponse> AddMarketingTag(int id, [FromBody] CreateTourCategoryDto dto)
        {
            _logger.LogInformation("Adding marketing tag to tour ID: {TourId}, Category ID: {CategoryId}", id, dto.CategoryId);
            var adminUsername = GetAdminUsername();
            return await _tourService.AddMarketingTagAsync(id, dto.CategoryId, adminUsername);
        }

        /// <summary>
        /// Remove marketing tag from tour
        /// </summary>
        /// <param name="id">Tour ID</param>
        /// <param name="categoryId">Category ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("{id}/marketing-tag/{categoryId}")]
        public async Task<ApiResponse> RemoveMarketingTag(int id, int categoryId)
        {
            _logger.LogInformation("Removing marketing tag from tour ID: {TourId}, Category ID: {CategoryId}", id, categoryId);
            var adminUsername = GetAdminUsername();
            return await _tourService.RemoveMarketingTagAsync(id, categoryId, adminUsername);
        }

        /// <summary>
        /// Get marketing tags for tour
        /// </summary>
        /// <param name="id">Tour ID</param>
        /// <returns>List of marketing tags</returns>
        [HttpGet("{id}/marketing-tags")]
        public async Task<ApiResponse> GetMarketingTags(int id)
        {
            _logger.LogInformation("Getting marketing tags for tour ID: {TourId}", id);
            return await _tourService.GetMarketingTagsAsync(id);
        }

        #endregion

        #region TourImage Operations

        /// <summary>
        /// Create tour image
        /// </summary>
        /// <param name="dto">Tour image data</param>
        /// <returns>Created tour image</returns>
        [HttpPost("image")]
        public async Task<ApiResponse> CreateTourImage([FromBody] CreateTourImageDto dto)
        {
            _logger.LogInformation("Creating tour image for tour ID: {TourId}", dto.TourId);
            var adminUsername = GetAdminUsername();
            return await _tourService.CreateTourImageAsync(dto, adminUsername);
        }

        /// <summary>
        /// Update tour image
        /// </summary>
        /// <param name="id">Image ID</param>
        /// <param name="dto">Image update data</param>
        /// <returns>Updated tour image</returns>
        [HttpPut("image/{id}")]
        public async Task<ApiResponse> UpdateTourImage(int id, [FromBody] UpdateTourImageDto dto)
        {
            _logger.LogInformation("Updating tour image with ID: {ImageId}", id);
            var adminUsername = GetAdminUsername();
            return await _tourService.UpdateTourImageAsync(id, dto, adminUsername);
        }

        /// <summary>
        /// Delete tour image
        /// </summary>
        /// <param name="id">Image ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("image/{id}")]
        public async Task<ApiResponse> DeleteTourImage(int id)
        {
            _logger.LogInformation("Deleting tour image with ID: {ImageId}", id);
            var adminUsername = GetAdminUsername();
            return await _tourService.DeleteTourImageAsync(id, adminUsername);
        }

        /// <summary>
        /// Get tour images
        /// </summary>
        /// <param name="tourId">Tour ID</param>
        /// <returns>List of tour images</returns>
        [HttpGet("image/tour/{tourId}")]
        public async Task<ApiResponse> GetTourImages(int tourId)
        {
            _logger.LogInformation("Getting images for tour ID: {TourId}", tourId);
            return await _tourService.GetTourImagesAsync(tourId);
        }

        #endregion

        #region TourCategory Operations

        /// <summary>
        /// Create tour category link
        /// </summary>
        /// <param name="dto">Tour category data</param>
        /// <returns>Created tour category</returns>
        [HttpPost("category")]
        public async Task<ApiResponse> CreateTourCategory([FromBody] CreateTourCategoryDto dto)
        {
            _logger.LogInformation("Creating tour category for tour ID: {TourId}, Category ID: {CategoryId}", dto.TourId, dto.CategoryId);
            var adminUsername = GetAdminUsername();
            return await _tourService.CreateTourCategoryAsync(dto, adminUsername);
        }

        /// <summary>
        /// Delete tour category link
        /// </summary>
        /// <param name="id">Tour category ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("category/{id}")]
        public async Task<ApiResponse> DeleteTourCategory(int id)
        {
            _logger.LogInformation("Deleting tour category with ID: {CategoryId}", id);
            var adminUsername = GetAdminUsername();
            return await _tourService.DeleteTourCategoryAsync(id, adminUsername);
        }

        /// <summary>
        /// Get tour categories
        /// </summary>
        /// <param name="tourId">Tour ID</param>
        /// <returns>List of tour categories</returns>
        [HttpGet("category/tour/{tourId}")]
        public async Task<ApiResponse> GetTourCategories(int tourId)
        {
            _logger.LogInformation("Getting categories for tour ID: {TourId}", tourId);
            return await _tourService.GetTourCategoriesAsync(tourId);
        }

        #endregion

        #region TourPrice Operations

        /// <summary>
        /// Create tour price
        /// </summary>
        /// <param name="dto">Tour price data</param>
        /// <returns>Created tour price</returns>
        [HttpPost("price")]
        public async Task<ApiResponse> CreateTourPrice([FromBody] CreateTourPriceDto dto)
        {
            _logger.LogInformation("Creating tour price for tour ID: {TourId}, Price Type ID: {PriceTypeId}", dto.TourId, dto.PriceTypeId);
            var adminUsername = GetAdminUsername();
            return await _tourService.CreateTourPriceAsync(dto, adminUsername);
        }

        /// <summary>
        /// Update tour price
        /// </summary>
        /// <param name="id">Price ID</param>
        /// <param name="dto">Price update data</param>
        /// <returns>Updated tour price</returns>
        [HttpPut("price/{id}")]
        public async Task<ApiResponse> UpdateTourPrice(int id, [FromBody] UpdateTourPriceDto dto)
        {
            _logger.LogInformation("Updating tour price with ID: {PriceId}", id);
            var adminUsername = GetAdminUsername();
            return await _tourService.UpdateTourPriceAsync(id, dto, adminUsername);
        }

        /// <summary>
        /// Delete tour price
        /// </summary>
        /// <param name="id">Price ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("price/{id}")]
        public async Task<ApiResponse> DeleteTourPrice(int id)
        {
            _logger.LogInformation("Deleting tour price with ID: {PriceId}", id);
            var adminUsername = GetAdminUsername();
            return await _tourService.DeleteTourPriceAsync(id, adminUsername);
        }

        /// <summary>
        /// Get tour prices
        /// </summary>
        /// <param name="tourId">Tour ID</param>
        /// <returns>List of tour prices</returns>
        [HttpGet("price/tour/{tourId}")]
        public async Task<ApiResponse> GetTourPrices(int tourId)
        {
            _logger.LogInformation("Getting prices for tour ID: {TourId}", tourId);
            return await _tourService.GetTourPricesAsync(tourId);
        }

        #endregion

        #region TourPolicy Operations

        /// <summary>
        /// Create tour policy link
        /// </summary>
        /// <param name="dto">Tour policy data</param>
        /// <returns>Created tour policy</returns>
        [HttpPost("policy")]
        public async Task<ApiResponse> CreateTourPolicy([FromBody] CreateTourPolicyDto dto)
        {
            _logger.LogInformation("Creating tour policy for tour ID: {TourId}, Policy ID: {PolicyId}", dto.TourId, dto.PolicyId);
            var adminUsername = GetAdminUsername();
            return await _tourService.CreateTourPolicyAsync(dto, adminUsername);
        }

        /// <summary>
        /// Delete tour policy link
        /// </summary>
        /// <param name="id">Tour policy ID</param>
        /// <returns>Success message</returns>
        [HttpDelete("policy/{id}")]
        public async Task<ApiResponse> DeleteTourPolicy(int id)
        {
            _logger.LogInformation("Deleting tour policy with ID: {PolicyId}", id);
            var adminUsername = GetAdminUsername();
            return await _tourService.DeleteTourPolicyAsync(id, adminUsername);
        }

        /// <summary>
        /// Get tour policies
        /// </summary>
        /// <param name="tourId">Tour ID</param>
        /// <returns>List of tour policies</returns>
        [HttpGet("policy/tour/{tourId}")]
        public async Task<ApiResponse> GetTourPolicies(int tourId)
        {
            _logger.LogInformation("Getting policies for tour ID: {TourId}", tourId);
            return await _tourService.GetTourPoliciesAsync(tourId);
        }

        #endregion

        #region Private Helper Methods

        private string GetAdminUsername()
        {
            return GetUserIdFromClaims.GetUsername(User);
        }

        #endregion
    }
}
