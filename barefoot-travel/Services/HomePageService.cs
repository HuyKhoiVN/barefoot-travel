using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Category;
using barefoot_travel.Models;
using barefoot_travel.Repositories;
using System.Text.Json;

namespace barefoot_travel.Services
{
    public class HomePageService : IHomePageService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITourRepository _tourRepository;
        private readonly ILogger<HomePageService> _logger;

        public HomePageService(
            ICategoryRepository categoryRepository,
            ITourRepository tourRepository,
            ILogger<HomePageService> logger)
        {
            _categoryRepository = categoryRepository;
            _tourRepository = tourRepository;
            _logger = logger;
        }

        public async Task<HomepageDataDto> GetHomepageSectionsAsync()
        {
            var categories = await _categoryRepository.GetCategoriesWithHomepageConfigAsync();
            var sections = new List<HomepageSectionDto>();

            foreach (var category in categories)
            {
                var config = ParseHomepageConfig(category.HomepageConfig);
                if (config == null || !config.IsActive) continue;

                // Business logic: Validate spotlight constraint
                ValidateSpotlightConfig(config, category.Id);

                // Get tours for this category
                var tours = await _tourRepository.GetToursByCategoryForHomepageAsync(category.Id, config.MaxItems);

                sections.Add(new HomepageSectionDto
                {
                    CategoryId = category.Id,
                    CategoryName = category.CategoryName,
                    HomepageTitle = category.HomepageTitle ?? category.CategoryName,
                    Type = category.Type,
                    TourCount = tours.Count,
                    Config = config,
                    Tours = tours
                });
            }

            return new HomepageDataDto { Sections = sections };
        }

        public async Task ConfigureCategoryHomepageAsync(int categoryId, ConfigureHomepageDto dto, string userId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new InvalidOperationException("Category not found");

            // Business logic: Validate layout style
            ValidateLayoutStyle(dto.LayoutStyle);

            // Business logic: Enforce spotlight constraint
            EnforceSpotlightConstraint(dto);

            var config = new HomepageConfigDto
            {
                LayoutStyle = dto.LayoutStyle,
                MaxItems = dto.MaxItems,
                IsActive = dto.IsActive,
                BadgeText = dto.BadgeText,
                CustomClass = dto.CustomClass,
                DisplayOrder = dto.DisplayOrder,
                SpotlightImageUrl = dto.SpotlightImageUrl
            };

            var configJson = JsonSerializer.Serialize(config);

            // Update entity
            UpdateCategoryForHomepage(category, dto.HomepageTitle, configJson, dto.DisplayOrder, userId);

            await _categoryRepository.UpdateAsync(category);
        }

        public async Task RemoveCategoryFromHomepageAsync(int categoryId, string userId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new InvalidOperationException("Category not found");

            category.HomepageTitle = null;
            category.HomepageConfig = null;
            category.HomepageOrder = null;
            category.UpdatedTime = DateTime.UtcNow;
            category.UpdatedBy = userId;

            await _categoryRepository.UpdateAsync(category);
        }

        public async Task ReorderSectionsAsync(List<ReorderSectionDto> sections, string userId)
        {
            foreach (var section in sections)
            {
                var category = await _categoryRepository.GetByIdAsync(section.CategoryId);
                if (category != null)
                {
                    category.HomepageOrder = section.DisplayOrder;
                    category.UpdatedTime = DateTime.UtcNow;
                    category.UpdatedBy = userId;
                    await _categoryRepository.UpdateAsync(category);
                }
            }
        }

        public async Task<object> GetCategoryHomepageConfigAsync(int categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new InvalidOperationException("Category not found");

            var config = ParseHomepageConfig(category.HomepageConfig);

            return new
            {
                CategoryId = category.Id,
                CategoryName = category.CategoryName,
                HomepageTitle = category.HomepageTitle,
                HomepageOrder = category.HomepageOrder,
                Config = config
            };
        }

        public async Task<WaysToTravelConfigDto> GetWaysToTravelCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var waysToTravelCategories = categories
                .Where(c => c.ShowInWaysToTravel == true && c.Active)
                .OrderBy(c => c.WaysToTravelOrder)
                .ToList();

            var result = new List<WaysToTravelCategoryDto>();

            foreach (var category in waysToTravelCategories)
            {
                var totalTours = await _tourRepository.GetTourCountByCategoryAsync(category.Id);

                if (!string.IsNullOrEmpty(category.WaysToTravelImage1))
                {
                    result.Add(new WaysToTravelCategoryDto
                    {
                        CategoryId = category.Id,
                        CategoryName = category.CategoryName,
                        TotalTours = totalTours,
                        ImageUrl1 = category.WaysToTravelImage1,
                        ImageUrl2 = category.WaysToTravelImage2,
                        DisplayOrder = category.WaysToTravelOrder ?? 0
                    });
                }
            }

            return new WaysToTravelConfigDto { Categories = result };
        }

        public async Task ConfigureCategoryForWaysToTravelAsync(int categoryId, ConfigureWaysToTravelDto dto, string userId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new InvalidOperationException("Category not found");

            // Business logic: Check maximum 5 categories
            await ValidateWaysToTravelLimit(dto.ShowInWaysToTravel, category.ShowInWaysToTravel);

            // Business logic: Validate Image1 is required
            ValidateWaysToTravelImage(dto);

            // Update entity
            UpdateCategoryForWaysToTravel(category, dto, userId);

            await _categoryRepository.UpdateAsync(category);
        }

        public async Task RemoveCategoryFromWaysToTravelAsync(int categoryId, string userId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new InvalidOperationException("Category not found");

            category.ShowInWaysToTravel = false;
            category.UpdatedTime = DateTime.UtcNow;
            category.UpdatedBy = userId;

            await _categoryRepository.UpdateAsync(category);
        }

        public async Task ReorderWaysToTravelCategoriesAsync(List<ReorderSectionDto> orders, string userId)
        {
            foreach (var order in orders)
            {
                var category = await _categoryRepository.GetByIdAsync(order.CategoryId);
                if (category != null)
                {
                    category.WaysToTravelOrder = order.DisplayOrder;
                    category.UpdatedTime = DateTime.UtcNow;
                    category.UpdatedBy = userId;
                    await _categoryRepository.UpdateAsync(category);
                }
            }
        }

        #region Private Helper Methods

        private HomepageConfigDto? ParseHomepageConfig(string? configJson)
        {
            if (string.IsNullOrEmpty(configJson))
                return null;

            return JsonSerializer.Deserialize<HomepageConfigDto>(
                configJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private void ValidateSpotlightConfig(HomepageConfigDto config, int categoryId)
        {
            if (config.LayoutStyle == "spotlight" && config.MaxItems > 3)
            {
                config.MaxItems = 3;
                _logger.LogWarning($"Category {categoryId} has spotlight layout but maxItems > 3. Limited to 3.");
            }
        }

        private void ValidateLayoutStyle(string layoutStyle)
        {
            var validLayouts = new[] { "grid", "grid-2", "grid-3", "spotlight", "carousel" };
            if (!validLayouts.Contains(layoutStyle))
                throw new InvalidOperationException("Invalid layout style");
        }

        private void EnforceSpotlightConstraint(ConfigureHomepageDto dto)
        {
            if (dto.LayoutStyle == "spotlight" && dto.MaxItems > 3)
            {
                dto.MaxItems = 3;
                _logger.LogWarning("Spotlight layout limited to 3 items");
            }
        }

        private async Task ValidateWaysToTravelLimit(bool showInWaysToTravel, bool? currentShowInWaysToTravel)
        {
            if (showInWaysToTravel)
            {
                var categories = await _categoryRepository.GetAllAsync();
                var waysToTravelCount = categories.Count(c => c.ShowInWaysToTravel == true && c.Active);

                if (waysToTravelCount >= 5 && currentShowInWaysToTravel != true)
                    throw new InvalidOperationException("Maximum 5 categories allowed in Ways to Travel section");
            }
        }

        private void ValidateWaysToTravelImage(ConfigureWaysToTravelDto dto)
        {
            if (dto.ShowInWaysToTravel && string.IsNullOrEmpty(dto.ImageUrl1))
                throw new InvalidOperationException("At least one image (Image1) is required for Ways to Travel");
        }

        private void UpdateCategoryForHomepage(Category category, string? homepageTitle, string configJson, int displayOrder, string userId)
        {
            category.HomepageTitle = homepageTitle;
            category.HomepageConfig = configJson;
            category.HomepageOrder = displayOrder;
            category.UpdatedTime = DateTime.UtcNow;
            category.UpdatedBy = userId;
        }

        private void UpdateCategoryForWaysToTravel(Category category, ConfigureWaysToTravelDto dto, string userId)
        {
            category.WaysToTravelImage1 = dto.ImageUrl1;
            category.WaysToTravelImage2 = dto.ImageUrl2;
            category.WaysToTravelOrder = dto.DisplayOrder;
            category.ShowInWaysToTravel = dto.ShowInWaysToTravel;
            category.UpdatedTime = DateTime.UtcNow;
            category.UpdatedBy = userId;
        }

        #endregion
    }
}
