using barefoot_travel.Common;
using barefoot_travel.Common.Exceptions;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Tour;
using barefoot_travel.Models;
using barefoot_travel.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace barefoot_travel.Services
{
    public class TourService : ITourService
    {
        private readonly ITourRepository _tourRepository;
        private readonly ITourImageRepository _tourImageRepository;
        private readonly ITourCategoryRepository _tourCategoryRepository;
        private readonly ITourPriceRepository _tourPriceRepository;
        private readonly ITourPolicyRepository _tourPolicyRepository;
        private readonly SysDbContext _context;

        public TourService(
            ITourRepository tourRepository,
            ITourImageRepository tourImageRepository,
            ITourCategoryRepository tourCategoryRepository,
            ITourPriceRepository tourPriceRepository,
            ITourPolicyRepository tourPolicyRepository,
            SysDbContext context)
        {
            _tourRepository = tourRepository;
            _tourImageRepository = tourImageRepository;
            _tourCategoryRepository = tourCategoryRepository;
            _tourPriceRepository = tourPriceRepository;
            _tourPolicyRepository = tourPolicyRepository;
            _context = context;
        }

        #region Tour CRUD Operations

        public async Task<ApiResponse> GetTourByIdAsync(int id)
        {
            try
            {
                var tourDetail = await _tourRepository.GetTourDetailByIdAsync(id);
                if (tourDetail == null)
                {
                    return new ApiResponse(false, "Tour not found");
                }

                return new ApiResponse(true, "Tour retrieved successfully", tourDetail);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving tour: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetTourBySlugAsync(string slug)
        {
            try
            {
                var tourDetail = await _tourRepository.GetTourDetailBySlugAsync(slug);
                if (tourDetail == null)
                {
                    return new ApiResponse(false, "Tour not found");
                }

                return new ApiResponse(true, "Tour retrieved successfully", tourDetail);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving tour: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetAllToursAsync()
        {
            try
            {
                var tours = await _tourRepository.GetToursWithBasicInfoAsync();
                return new ApiResponse(true, "Tours retrieved successfully", tours);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving tours: {ex.Message}");
            }
        }

        public async Task<List<DTOs.HomepageTourDto>> GetToursByCategoryForHomepageAsync(int categoryId, int maxItems)
        {
            try
            {
                var tours = await _tourRepository.GetToursByCategoryForHomepageAsync(categoryId, maxItems);
                return tours;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving tours by category: {ex.Message}");
            }
        }

        public async Task<PagedResult<TourDto>> GetToursPagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", List<int>? categoryIds = null, string? search = null, bool? active = null)
        {
            try
            {
                return await _tourRepository.GetToursPagedWithBasicInfoAsync(page, pageSize, sortBy, sortOrder, categoryIds, search, active);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving paged tours: {ex.Message}");
            }
        }

        public async Task<PagedResult<TourDto>> GetToursByCategoryPagedAsync(int categoryId, int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", string? search = null)
        {
            try
            {
                // Get all child category IDs recursively
                var categoryRepository = _context.Set<Category>();
                var childCategoryIds = await GetDescendantCategoryIdsRecursive(categoryId);
                
                // Include the parent category itself
                childCategoryIds.Add(categoryId);
                
                // Get tours that belong to this category or any of its children
                return await _tourRepository.GetToursPagedWithBasicInfoAsync(
                    page, 
                    pageSize, 
                    sortBy, 
                    sortOrder, 
                    childCategoryIds, 
                    search, 
                    true, // Only active tours for public view
                    Common.TourStatusConstant.Public); // Only public status tours
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving paged tours by category: {ex.Message}");
            }
        }

        /// <summary>
        /// Recursively get all descendant category IDs
        /// </summary>
        private async Task<List<int>> GetDescendantCategoryIdsRecursive(int parentId)
        {
            var descendantIds = new List<int>();
            
            var directChildren = await _context.Categories
                .Where(c => c.ParentId == parentId && c.Active)
                .Select(c => c.Id)
                .ToListAsync();

            foreach (var childId in directChildren)
            {
                descendantIds.Add(childId);
                var grandChildren = await GetDescendantCategoryIdsRecursive(childId);
                descendantIds.AddRange(grandChildren);
            }

            return descendantIds;
        }

        public async Task<ApiResponse> CreateTourAsync(CreateTourDto dto, string adminUsername)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate title uniqueness
                if (await _tourRepository.TitleExistsAsync(dto.Title))
                {
                    return new ApiResponse(false, "Tour title already exists");
                }

                // Generate slug if not provided
                var slug = string.IsNullOrWhiteSpace(dto.Slug) 
                    ? SlugGenerator.GenerateSlug(dto.Title) 
                    : SlugGenerator.GenerateSlug(dto.Slug);

                // Only set slug if generation was successful
                if (!string.IsNullOrWhiteSpace(slug))
                {
                    // Validate slug format
                    if (!SlugGenerator.IsValidSlug(slug))
                    {
                        return new ApiResponse(false, "Invalid slug format");
                    }

                    // Ensure slug is unique
                    if (await _tourRepository.SlugExistsAsync(slug))
                    {
                        var existingSlugs = await _tourRepository.GetAllSlugsAsync();
                        slug = SlugGenerator.EnsureUnique(slug, existingSlugs);
                    }
                }

                // Create tour
                var tour = MapToTour(dto, adminUsername);
                tour.Slug = string.IsNullOrWhiteSpace(slug) ? null : slug;
                var createdTour = await _tourRepository.CreateAsync(tour);

                // Create related data within transaction
                await CreateTourRelatedDataAsync(createdTour.Id, dto, adminUsername);

                await transaction.CommitAsync();

                var tourDto = MapToTourDto(createdTour);
                return new ApiResponse(true, "Tour created successfully", tourDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse(false, $"Error creating tour: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateTourAsync(int id, UpdateTourDto dto, string adminUsername)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var tour = await _tourRepository.GetByIdAsync(id);
                if (tour == null)
                {
                    return new ApiResponse(false, "Tour not found");
                }

                // Validate title uniqueness (excluding current tour)
                if (await _tourRepository.TitleExistsAsync(dto.Title, id))
                {
                    return new ApiResponse(false, "Tour title already exists");
                }

                // Handle slug update
                if (!string.IsNullOrWhiteSpace(dto.Slug))
                {
                    var newSlug = SlugGenerator.GenerateSlug(dto.Slug);
                    
                    // Only update if slug generation was successful
                    if (!string.IsNullOrWhiteSpace(newSlug))
                    {
                        // Validate slug format
                        if (!SlugGenerator.IsValidSlug(newSlug))
                        {
                            await transaction.RollbackAsync();
                            return new ApiResponse(false, "Invalid slug format");
                        }
                        
                        // Check if slug changed and new slug is unique
                        if (newSlug != tour.Slug && 
                            await _tourRepository.SlugExistsAsync(newSlug, id))
                        {
                            await transaction.RollbackAsync();
                            return new ApiResponse(false, "Slug already exists");
                        }
                        
                        tour.Slug = newSlug;
                    }
                }
                else if (dto.Title != tour.Title)
                {
                    // Auto-update slug if title changed but slug not provided
                    var newSlug = SlugGenerator.GenerateSlug(dto.Title);
                    if (!string.IsNullOrWhiteSpace(newSlug) && 
                        !await _tourRepository.SlugExistsAsync(newSlug, id))
                    {
                        tour.Slug = newSlug;
                    }
                }

                // Update tour basic info
                MapToTourForUpdate(tour, dto, adminUsername);
                await _tourRepository.UpdateAsync(tour);

                // Update related data within transaction
                await UpdateTourRelatedDataAsync(id, dto, adminUsername);

                await transaction.CommitAsync();

                var tourDto = MapToTourDto(tour);
                return new ApiResponse(true, "Tour updated successfully", tourDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ApiResponse(false, $"Error updating tour: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteTourAsync(int id, string adminUsername)
        {
            try
            {
                // Check for active bookings
                if (await _tourRepository.HasActiveBookingsAsync(id))
                {
                    return new ApiResponse(false, "Cannot delete tour with active bookings");
                }

                var success = await _tourRepository.DeleteAsync(id);
                if (!success)
                {
                    return new ApiResponse(false, "Tour not found");
                }

                return new ApiResponse(true, "Tour deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error deleting tour: {ex.Message}");
            }
        }

        #endregion

        #region Tour Status Operations

        public async Task<ApiResponse> UpdateTourStatusAsync(int id, bool active, string adminUsername)
        {
            try
            {
                // Check for active bookings when disabling
                if (!active && await _tourRepository.HasActiveBookingsAsync(id))
                {
                    return new ApiResponse(false, "Cannot disable tour with active bookings");
                }

                var success = await _tourRepository.UpdateStatusAsync(id, active, adminUsername);
                if (!success)
                {
                    return new ApiResponse(false, "Tour not found");
                }

                var statusDto = new TourStatusDto { Id = id, Active = active, UpdatedTime = DateTime.UtcNow };
                return new ApiResponse(true, "Tour status updated successfully", statusDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error updating tour status: {ex.Message}");
            }
        }

        #endregion

        #region Tour Itinerary Operations

        public async Task<ApiResponse> UpdateTourItineraryAsync(int id, string itineraryJson, string adminUsername)
        {
            try
            {
                var tour = await _tourRepository.GetByIdAsync(id);
                if (tour == null)
                {
                    return new ApiResponse(false, "Tour not found");
                }

                tour.Description = itineraryJson;
                tour.UpdatedTime = DateTime.UtcNow;
                tour.UpdatedBy = adminUsername;
                await _tourRepository.UpdateAsync(tour);

                var itineraryDto = new TourItineraryDto { Id = id, ItineraryJson = itineraryJson, UpdatedTime = DateTime.UtcNow };
                return new ApiResponse(true, "Tour itinerary updated successfully", itineraryDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error updating tour itinerary: {ex.Message}");
            }
        }

        #endregion

        #region Marketing Tag Operations

        public async Task<ApiResponse> AddMarketingTagAsync(int tourId, int categoryId, string adminUsername)
        {
            try
            {
                // Validate tour exists
                if (!await _tourRepository.TourExistsAsync(tourId))
                {
                    return new ApiResponse(false, "Tour not found");
                }

                // Validate category exists and is marketing type
                if (!await _tourRepository.CategoryExistsAsync(categoryId))
                {
                    return new ApiResponse(false, "Category not found");
                }

                if (!await _tourRepository.IsMarketingCategoryAsync(categoryId))
                {
                    return new ApiResponse(false, "Invalid marketing category");
                }

                // Check if link already exists
                if (await _tourRepository.CategoryLinkExistsAsync(tourId, categoryId))
                {
                    return new ApiResponse(false, "Marketing tag already exists for this tour");
                }

                var tourCategory = new TourCategory
                {
                    TourId = tourId,
                    CategoryId = categoryId,
                    CreatedTime = DateTime.UtcNow,
                    Active = true
                };

                var marketingTagDto = await _tourCategoryRepository.CreateMarketingTagAsync(tourCategory);
                return new ApiResponse(true, "Marketing tag added successfully", marketingTagDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error adding marketing tag: {ex.Message}");
            }
        }

        public async Task<ApiResponse> RemoveMarketingTagAsync(int tourId, int categoryId, string adminUsername)
        {
            try
            {
                var success = await _tourCategoryRepository.DeleteMarketingTagAsync(tourId, categoryId);
                if (!success)
                {
                    return new ApiResponse(false, "Marketing tag not found");
                }

                return new ApiResponse(true, "Marketing tag removed successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error removing marketing tag: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetMarketingTagsAsync(int tourId)
        {
            try
            {
                var marketingTags = await _tourCategoryRepository.GetMarketingTagsByTourIdAsync(tourId);
                return new ApiResponse(true, "Marketing tags retrieved successfully", marketingTags);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving marketing tags: {ex.Message}");
            }
        }

        #endregion

        #region TourImage Operations

        public async Task<ApiResponse> CreateTourImageAsync(CreateTourImageDto dto, string adminUsername)
        {
            try
            {
                // Validate tour exists
                if (!await _tourRepository.TourExistsAsync(dto.TourId))
                {
                    return new ApiResponse(false, "Tour not found");
                }

                var tourImage = new TourImage
                {
                    TourId = dto.TourId,
                    ImageUrl = dto.ImageUrl,
                    IsBanner = dto.IsBanner,
                    CreatedTime = DateTime.UtcNow,
                    Active = true
                };

                var imageDto = await _tourImageRepository.CreateAsync(tourImage);
                return new ApiResponse(true, "Tour image created successfully", imageDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error creating tour image: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateTourImageAsync(int id, UpdateTourImageDto dto, string adminUsername)
        {
            try
            {
                var tourImage = await _tourImageRepository.GetByIdAsync(id);
                if (tourImage == null)
                {
                    return new ApiResponse(false, "Tour image not found");
                }

                var image = new TourImage
                {
                    Id = id,
                    TourId = tourImage.TourId,
                    ImageUrl = dto.ImageUrl,
                    IsBanner = dto.IsBanner,
                    CreatedTime = tourImage.CreatedTime,
                    UpdatedTime = DateTime.UtcNow,
                    UpdatedBy = adminUsername,
                    Active = true
                };

                var imageDto = await _tourImageRepository.UpdateAsync(image);
                return new ApiResponse(true, "Tour image updated successfully", imageDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error updating tour image: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteTourImageAsync(int id, string adminUsername)
        {
            try
            {
                var success = await _tourImageRepository.DeleteAsync(id);
                if (!success)
                {
                    return new ApiResponse(false, "Tour image not found");
                }

                return new ApiResponse(true, "Tour image deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error deleting tour image: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetTourImagesAsync(int tourId)
        {
            try
            {
                var images = await _tourImageRepository.GetByTourIdAsync(tourId);
                return new ApiResponse(true, "Tour images retrieved successfully", images);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving tour images: {ex.Message}");
            }
        }

        #endregion

        #region TourCategory Operations

        public async Task<ApiResponse> CreateTourCategoryAsync(CreateTourCategoryDto dto, string adminUsername)
        {
            try
            {
                // Validate tour exists
                if (!await _tourRepository.TourExistsAsync(dto.TourId))
                {
                    return new ApiResponse(false, "Tour not found");
                }

                // Validate category exists
                if (!await _tourRepository.CategoryExistsAsync(dto.CategoryId))
                {
                    return new ApiResponse(false, "Category not found");
                }

                // Check if link already exists
                if (await _tourRepository.CategoryLinkExistsAsync(dto.TourId, dto.CategoryId))
                {
                    return new ApiResponse(false, "Tour category link already exists");
                }

                var tourCategory = new TourCategory
                {
                    TourId = dto.TourId,
                    CategoryId = dto.CategoryId,
                    CreatedTime = DateTime.UtcNow,
                    Active = true
                };

                var categoryDto = await _tourCategoryRepository.CreateAsync(tourCategory);
                return new ApiResponse(true, "Tour category created successfully", categoryDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error creating tour category: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteTourCategoryAsync(int id, string adminUsername)
        {
            try
            {
                var success = await _tourCategoryRepository.DeleteAsync(id);
                if (!success)
                {
                    return new ApiResponse(false, "Tour category not found");
                }

                return new ApiResponse(true, "Tour category deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error deleting tour category: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetTourCategoriesAsync(int tourId)
        {
            try
            {
                var tourCategories = await _tourCategoryRepository.GetByTourIdAsync(tourId);
                return new ApiResponse(true, "Tour categories retrieved successfully", tourCategories);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving tour categories: {ex.Message}");
            }
        }

        #endregion

        #region TourPrice Operations

        public async Task<ApiResponse> CreateTourPriceAsync(CreateTourPriceDto dto, string adminUsername)
        {
            try
            {
                // Validate tour exists
                if (!await _tourRepository.TourExistsAsync(dto.TourId))
                {
                    return new ApiResponse(false, "Tour not found");
                }

                // Validate price type exists
                if (!await _tourRepository.PriceTypeExistsAsync(dto.PriceTypeId))
                {
                    return new ApiResponse(false, "Price type not found");
                }

                var tourPrice = new TourPrice
                {
                    TourId = dto.TourId,
                    PriceTypeId = dto.PriceTypeId,
                    Price = dto.Price,
                    CreatedTime = DateTime.UtcNow,
                    Active = true
                };

                var priceDto = await _tourPriceRepository.CreateAsync(tourPrice);
                return new ApiResponse(true, "Tour price created successfully", priceDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error creating tour price: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateTourPriceAsync(int id, UpdateTourPriceDto dto, string adminUsername)
        {
            try
            {
                var tourPrice = await _tourPriceRepository.GetByIdAsync(id);
                if (tourPrice == null)
                {
                    return new ApiResponse(false, "Tour price not found");
                }

                var price = new TourPrice
                {
                    Id = id,
                    TourId = tourPrice.TourId,
                    PriceTypeId = tourPrice.PriceTypeId,
                    Price = dto.Price,
                    CreatedTime = tourPrice.CreatedTime,
                    UpdatedTime = DateTime.UtcNow,
                    UpdatedBy = adminUsername,
                    Active = true
                };

                var priceDto = await _tourPriceRepository.UpdateAsync(price);
                return new ApiResponse(true, "Tour price updated successfully", priceDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error updating tour price: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteTourPriceAsync(int id, string adminUsername)
        {
            try
            {
                var success = await _tourPriceRepository.DeleteAsync(id);
                if (!success)
                {
                    return new ApiResponse(false, "Tour price not found");
                }

                return new ApiResponse(true, "Tour price deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error deleting tour price: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetTourPricesAsync(int tourId)
        {
            try
            {
                var tourPrices = await _tourPriceRepository.GetByTourIdAsync(tourId);
                return new ApiResponse(true, "Tour prices retrieved successfully", tourPrices);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving tour prices: {ex.Message}");
            }
        }

        #endregion

        #region TourPolicy Operations

        public async Task<ApiResponse> CreateTourPolicyAsync(CreateTourPolicyDto dto, string adminUsername)
        {
            try
            {
                // Validate tour exists
                if (!await _tourRepository.TourExistsAsync(dto.TourId))
                {
                    return new ApiResponse(false, "Tour not found");
                }

                // Validate policy exists
                if (!await _tourRepository.PolicyExistsAsync(dto.PolicyId))
                {
                    return new ApiResponse(false, "Policy not found");
                }

                // Check if link already exists
                if (await _tourRepository.PolicyLinkExistsAsync(dto.TourId, dto.PolicyId))
                {
                    return new ApiResponse(false, "Tour policy link already exists");
                }

                var tourPolicy = new TourPolicy
                {
                    TourId = dto.TourId,
                    PolicyId = dto.PolicyId,
                    CreatedTime = DateTime.UtcNow,
                    Active = true
                };

                var policyDto = await _tourPolicyRepository.CreateAsync(tourPolicy);
                return new ApiResponse(true, "Tour policy created successfully", policyDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error creating tour policy: {ex.Message}");
            }
        }

        public async Task<ApiResponse> DeleteTourPolicyAsync(int id, string adminUsername)
        {
            try
            {
                var success = await _tourPolicyRepository.DeleteAsync(id);
                if (!success)
                {
                    return new ApiResponse(false, "Tour policy not found");
                }

                return new ApiResponse(true, "Tour policy deleted successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error deleting tour policy: {ex.Message}");
            }
        }

        public async Task<ApiResponse> GetTourPoliciesAsync(int tourId)
        {
            try
            {
                var tourPolicies = await _tourPolicyRepository.GetByTourIdAsync(tourId);
                return new ApiResponse(true, "Tour policies retrieved successfully", tourPolicies);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving tour policies: {ex.Message}");
            }
        }

        #endregion

        #region Tour Status Approval Operations

        public async Task<ApiResponse> ChangeStatusAsync(int tourId, string newStatus, string updatedBy, string? reason = null)
        {
            try
            {
                // Validate status
                if (!TourStatusConstant.IsValidStatus(newStatus))
                {
                    return new ApiResponse(false, "Invalid status value");
                }
                
                // Get current status
                var currentStatus = await _tourRepository.GetCurrentStatusAsync(tourId);
                if (currentStatus == null)
                {
                    return new ApiResponse(false, "Tour not found");
                }
                
                // Check if transition is allowed
                if (!TourStatusConstant.CanTransitionTo(currentStatus, newStatus))
                {
                    return new ApiResponse(false, $"Cannot transition from {TourStatusConstant.GetStatusDisplayName(currentStatus)} to {TourStatusConstant.GetStatusDisplayName(newStatus)}");
                }
                
                var result = await _tourRepository.ChangeStatusAsync(tourId, newStatus, updatedBy, reason);
                
                if (!result)
                {
                    return new ApiResponse(false, "Failed to change tour status");
                }
                
                return new ApiResponse(true, "Tour status changed successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"An error occurred while changing tour status: {ex.Message}");
            }
        }

        public async Task<PagedResult<TourWithStatusDto>> GetToursPagedByStatusAsync(
            string? status, 
            int page, 
            int pageSize, 
            string? sortBy = null, 
            string? sortOrder = "asc")
        {
            return await _tourRepository.GetToursPagedByStatusAsync(status, page, pageSize, sortBy, sortOrder);
        }

        public async Task<ApiResponse> GetStatusHistoryAsync(int tourId)
        {
            try
            {
                var history = await _tourRepository.GetStatusHistoryAsync(tourId);
                return new ApiResponse(true, "Status history retrieved successfully", history);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"An error occurred while getting status history: {ex.Message}");
            }
        }

        public async Task<ApiResponse> BatchChangeStatusAsync(
            List<int> tourIds, 
            string newStatus, 
            string updatedBy, 
            string? reason = null)
        {
            try
            {
                if (tourIds == null || !tourIds.Any())
                {
                    return new ApiResponse(false, "No tours selected");
                }
                
                if (!TourStatusConstant.IsValidStatus(newStatus))
                {
                    return new ApiResponse(false, "Invalid status value");
                }
                
                var result = await _tourRepository.BatchChangeStatusAsync(tourIds, newStatus, updatedBy, reason);
                
                if (result.SuccessCount == 0)
                {
                    return new ApiResponse(false, "Failed to change status for all tours", result);
                }
                
                if (result.FailCount > 0)
                {
                    return new ApiResponse(true, $"Status changed for {result.SuccessCount} tours, {result.FailCount} failed", result);
                }
                
                return new ApiResponse(true, $"Status changed successfully for {result.SuccessCount} tours", result);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"An error occurred during batch status change: {ex.Message}");
            }
        }

        public async Task<ApiResponse> BatchDeleteToursAsync(List<int> tourIds, string updatedBy)
        {
            try
            {
                if (tourIds == null || !tourIds.Any())
                {
                    return new ApiResponse(false, "No tours selected");
                }
                
                var result = await _tourRepository.BatchDeleteAsync(tourIds, updatedBy);
                
                if (result.SuccessCount == 0)
                {
                    return new ApiResponse(false, "Failed to delete all tours", result);
                }
                
                if (result.FailCount > 0)
                {
                    return new ApiResponse(true, $"Deleted {result.SuccessCount} tours, {result.FailCount} failed", result);
                }
                
                return new ApiResponse(true, $"Successfully deleted {result.SuccessCount} tours", result);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"An error occurred during batch delete: {ex.Message}");
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task CreateTourRelatedDataAsync(int tourId, CreateTourDto dto, string adminUsername)
        {
            // Create images
            if (dto.Images.Any())
            {
                foreach (var imageDto in dto.Images)
                {
                    var tourImage = new TourImage
                    {
                        TourId = tourId,
                        ImageUrl = imageDto.ImageUrl,
                        IsBanner = imageDto.IsBanner,
                        CreatedTime = DateTime.UtcNow,
                        Active = true
                    };
                    await _tourImageRepository.CreateAsync(tourImage);
                }
            }

            // Create categories with parent-child logic
            if (dto.Categories.Any())
            {
                var categoriesToAdd = await GetCategoriesWithParentsAsync(dto.Categories);
                foreach (var categoryId in categoriesToAdd)
                {
                    var tourCategory = new TourCategory
                    {
                        TourId = tourId,
                        CategoryId = categoryId,
                        CreatedTime = DateTime.UtcNow,
                        Active = true
                    };
                    await _tourCategoryRepository.CreateAsync(tourCategory);
                }
            }

            // Create prices
            if (dto.Prices.Any())
            {
                foreach (var priceDto in dto.Prices)
                {
                    var tourPrice = new TourPrice
                    {
                        TourId = tourId,
                        PriceTypeId = priceDto.PriceTypeId,
                        Price = priceDto.Price,
                        CreatedTime = DateTime.UtcNow,
                        Active = true
                    };
                    await _tourPriceRepository.CreateAsync(tourPrice);
                }
            }

            // Create policies
            if (dto.Policies.Any())
            {
                foreach (var policyId in dto.Policies)
                {
                    var tourPolicy = new TourPolicy
                    {
                        TourId = tourId,
                        PolicyId = policyId,
                        CreatedTime = DateTime.UtcNow,
                        Active = true
                    };
                    await _tourPolicyRepository.CreateAsync(tourPolicy);
                }
            }
        }

        private async Task UpdateTourRelatedDataAsync(int tourId, UpdateTourDto dto, string adminUsername)
        {
            // Update categories with parent-child logic
            if (dto.Categories != null)
            {
                // Remove existing categories
                await _tourCategoryRepository.DeleteByTourIdAsync(tourId);
                
                // Add new categories with parent logic
                if (dto.Categories.Any())
                {
                    var categoriesToAdd = await GetCategoriesWithParentsAsync(dto.Categories);
                    foreach (var categoryId in categoriesToAdd)
                    {
                        var tourCategory = new TourCategory
                        {
                            TourId = tourId,
                            CategoryId = categoryId,
                            CreatedTime = DateTime.UtcNow,
                            Active = true
                        };
                        await _tourCategoryRepository.CreateAsync(tourCategory);
                    }
                }
            }

            // Update policies
            if (dto.Policies != null)
            {
                // Remove existing policies
                await _tourPolicyRepository.DeleteByTourIdAsync(tourId);
                
                // Add new policies
                if (dto.Policies.Any())
                {
                    foreach (var policyId in dto.Policies)
                    {
                        var tourPolicy = new TourPolicy
                        {
                            TourId = tourId,
                            PolicyId = policyId,
                            CreatedTime = DateTime.UtcNow,
                            Active = true
                        };
                        await _tourPolicyRepository.CreateAsync(tourPolicy);
                    }
                }
            }
        }

        private async Task<List<int>> GetCategoriesWithParentsAsync(List<int> selectedCategoryIds)
        {
            var allCategories = await _context.Categories.ToListAsync();
            var categoriesToAdd = new HashSet<int>();

            foreach (var categoryId in selectedCategoryIds)
            {
                // Add the selected category
                categoriesToAdd.Add(categoryId);
                
                // Add all parent categories
                var category = allCategories.FirstOrDefault(c => c.Id == categoryId);
                while (category?.ParentId != null)
                {
                    categoriesToAdd.Add(category.ParentId.Value);
                    category = allCategories.FirstOrDefault(c => c.Id == category.ParentId.Value);
                }
            }

            return categoriesToAdd.ToList();
        }

        private Tour MapToTour(CreateTourDto dto, string adminUsername)
        {
            return new Tour
            {
                Title = dto.Title,
                Description = dto.Description,
                MapLink = dto.MapLink,
                PricePerPerson = dto.PricePerPerson,
                MaxPeople = dto.MaxPeople,
                Duration = dto.Duration,
                StartTime = dto.StartTime,
                ReturnTime = dto.ReturnTime,
                CreatedTime = DateTime.UtcNow,
                UpdatedBy = adminUsername,
                Active = true
            };
        }

        private void MapToTourForUpdate(Tour tour, UpdateTourDto dto, string adminUsername)
        {
            tour.Title = dto.Title;
            tour.Description = dto.Description;
            tour.MapLink = dto.MapLink;
            tour.PricePerPerson = dto.PricePerPerson;
            tour.MaxPeople = dto.MaxPeople;
            tour.Duration = dto.Duration;
            tour.StartTime = dto.StartTime;
            tour.ReturnTime = dto.ReturnTime;
            tour.UpdatedTime = DateTime.UtcNow;
            tour.UpdatedBy = adminUsername;
        }

        private TourDto MapToTourDto(Tour tour)
        {
            return new TourDto
            {
                Id = tour.Id,
                Title = tour.Title,
                Description = tour.Description,
                MapLink = tour.MapLink,
                PricePerPerson = tour.PricePerPerson,
                MaxPeople = tour.MaxPeople,
                Duration = tour.Duration,
                StartTime = tour.StartTime,
                ReturnTime = tour.ReturnTime,
                CreatedTime = tour.CreatedTime,
                UpdatedTime = tour.UpdatedTime,
                UpdatedBy = tour.UpdatedBy,
                Active = tour.Active
            };
        }

        #endregion

        #region Enhanced TourImage Operations

        public async Task<ApiResponse> UploadTourImageAsync(CreateTourImageDto dto, IFormFile file, string adminUsername)
        {
            try
            {
                // Upload file first
                var uploadResult = await UploadFileAsync(file);
                if (!uploadResult.Success)
                {
                    return uploadResult;
                }

                // Update DTO with uploaded file URL
                dto.ImageUrl = uploadResult.Data?.ToString() ?? "";

                // Create tour image record
                return await CreateTourImageAsync(dto, adminUsername);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error uploading tour image: {ex.Message}");
            }
        }

        public async Task<ApiResponse> SetTourImageAsBannerAsync(int imageId, string adminUsername)
        {
            try
            {
                var image = await _tourImageRepository.GetByIdAsync(imageId);
                if (image == null)
                {
                    return new ApiResponse(false, "Image not found");
                }

                await _tourImageRepository.UpdateBanner(imageId);

                return new ApiResponse(true, "Image set as banner successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error setting image as banner: {ex.Message}");
            }
        }

        public async Task<ApiResponse> RemoveTourImageBannerAsync(int imageId, string adminUsername)
        {
            try
            {
                await _tourImageRepository.RemoveBanner(imageId);
                return new ApiResponse(true, "Banner status removed successfully");
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error removing banner status: {ex.Message}");
            }
        }

        private async Task<ApiResponse> UploadFileAsync(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return new ApiResponse(false, "No file provided");
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return new ApiResponse(false, "Invalid file type. Only JPG, JPEG, PNG, GIF, and WebP files are allowed");
                }

                // Validate file size (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    return new ApiResponse(false, "File size too large. Maximum size is 10MB");
                }

                // Generate unique filename
                var fileName = GenerateUniqueFileName(file.FileName);

                // Create directory if it doesn't exist
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Save file
                var filePath = Path.Combine(uploadPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Generate URL
                var fileUrl = $"/uploads/{fileName}";

                return new ApiResponse(true, "File uploaded successfully", fileUrl);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error uploading file: {ex.Message}");
            }
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);

            // Generate hash from original name and timestamp
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var hash = ComputeHash(nameWithoutExtension + timestamp);

            return $"{hash}{extension}";
        }

        private string ComputeHash(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return Convert.ToHexString(hashBytes)[..8].ToLowerInvariant();
            }
        }

        #endregion
    }
}