using barefoot_travel.Common;
using barefoot_travel.Common.Exceptions;
using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Tour;
using barefoot_travel.Models;
using barefoot_travel.Repositories;

namespace barefoot_travel.Services
{
    public class TourService : ITourService
    {
        private readonly ITourRepository _tourRepository;

        public TourService(ITourRepository tourRepository)
        {
            _tourRepository = tourRepository;
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

        public async Task<PagedResult<TourDto>> GetToursPagedAsync(int page, int pageSize, string? sortBy = null, string? sortOrder = "asc", int? categoryId = null, bool? active = null)
        {
            try
            {
                return await _tourRepository.GetToursPagedWithBasicInfoAsync(page, pageSize, sortBy, sortOrder, categoryId, active);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving paged tours: {ex.Message}");
            }
        }

        public async Task<ApiResponse> CreateTourAsync(CreateTourDto dto, string adminUsername)
        {
            try
            {
                // Validate title uniqueness
                if (await _tourRepository.TitleExistsAsync(dto.Title))
                {
                    return new ApiResponse(false, "Tour title already exists");
                }

                // Create tour
                var tour = MapToTour(dto, adminUsername);
                var createdTour = await _tourRepository.CreateAsync(tour);

                // Create related data
                await CreateTourRelatedDataAsync(createdTour.Id, dto, adminUsername);

                var tourDto = MapToTourDto(createdTour);
                return new ApiResponse(true, "Tour created successfully", tourDto);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error creating tour: {ex.Message}");
            }
        }

        public async Task<ApiResponse> UpdateTourAsync(int id, UpdateTourDto dto, string adminUsername)
        {
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

                MapToTourForUpdate(tour, dto, adminUsername);
                await _tourRepository.UpdateAsync(tour);

                var tourDto = MapToTourDto(tour);
                return new ApiResponse(true, "Tour updated successfully", tourDto);
            }
            catch (Exception ex)
            {
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

                var marketingTagDto = await _tourRepository.CreateMarketingTagAsync(tourCategory);
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
                var success = await _tourRepository.DeleteMarketingTagAsync(tourId, categoryId);
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
                var marketingTags = await _tourRepository.GetMarketingTagsByTourIdAsync(tourId);
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

                var imageDto = await _tourRepository.CreateImageAsync(tourImage);
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
                var tourImage = await _tourRepository.GetImageByIdAsync(id);
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

                var imageDto = await _tourRepository.UpdateImageAsync(image);
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
                var success = await _tourRepository.DeleteImageAsync(id);
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
                var images = await _tourRepository.GetImagesByTourIdAsync(tourId);
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

                var categoryDto = await _tourRepository.CreateCategoryAsync(tourCategory);
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
                var success = await _tourRepository.DeleteCategoryAsync(id);
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
                var tourCategories = await _tourRepository.GetCategoriesByTourIdAsync(tourId);
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

                var priceDto = await _tourRepository.CreatePriceAsync(tourPrice);
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
                var tourPrice = await _tourRepository.GetPriceByIdAsync(id);
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

                var priceDto = await _tourRepository.UpdatePriceAsync(price);
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
                var success = await _tourRepository.DeletePriceAsync(id);
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
                var tourPrices = await _tourRepository.GetPricesByTourIdAsync(tourId);
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

                var policyDto = await _tourRepository.CreatePolicyAsync(tourPolicy);
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
                var success = await _tourRepository.DeletePolicyAsync(id);
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
                var tourPolicies = await _tourRepository.GetPoliciesByTourIdAsync(tourId);
                return new ApiResponse(true, "Tour policies retrieved successfully", tourPolicies);
            }
            catch (Exception ex)
            {
                return new ApiResponse(false, $"Error retrieving tour policies: {ex.Message}");
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
                    await _tourRepository.CreateImageAsync(tourImage);
                }
            }

            // Create categories
            if (dto.Categories.Any())
            {
                foreach (var categoryId in dto.Categories)
                {
                    var tourCategory = new TourCategory
                    {
                        TourId = tourId,
                        CategoryId = categoryId,
                        CreatedTime = DateTime.UtcNow,
                        Active = true
                    };
                    await _tourRepository.CreateCategoryAsync(tourCategory);
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
                    await _tourRepository.CreatePriceAsync(tourPrice);
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
                    await _tourRepository.CreatePolicyAsync(tourPolicy);
                }
            }
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
    }
}