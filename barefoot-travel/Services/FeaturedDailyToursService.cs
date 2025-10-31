using barefoot_travel.DTOs.Category;
using barefoot_travel.Models;
using barefoot_travel.Repositories;

namespace barefoot_travel.Services
{
    public class FeaturedDailyToursService : IFeaturedDailyToursService
    {
        private readonly IHomePageFeaturedTourRepository _featuredTourRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ITourRepository _tourRepository;

        public FeaturedDailyToursService(
            IHomePageFeaturedTourRepository featuredTourRepository,
            ICategoryRepository categoryRepository,
            ITourRepository tourRepository)
        {
            _featuredTourRepository = featuredTourRepository;
            _categoryRepository = categoryRepository;
            _tourRepository = tourRepository;
        }

        // Featured Tours
        public async Task<FeaturedToursConfigDto> GetFeaturedToursAsync()
        {
            var tours = await _featuredTourRepository.GetAllFeaturedToursAsync();

            var toursDto = tours.Select(t => new FeaturedTourDto
            {
                Id = t.Id,
                Badge = t.Category ?? string.Empty,
                CategoryName = t.Title ?? string.Empty,
                Description = t.Description ?? string.Empty,
                ImageUrl = t.ImageUrl ?? string.Empty,
                DisplayOrder = t.DisplayOrder,
                TourId = t.TourId,
                CardClass = t.CardClass
            }).ToList();

            return new FeaturedToursConfigDto
            {
                Tours = toursDto
            };
        }

        public async Task ConfigureFeaturedTourAsync(int tourId, ConfigureFeaturedTourDto dto, string userId)
        {
            // Validate tour exists
            var tour = await _tourRepository.GetByIdAsync(tourId);
            if (tour == null)
                throw new InvalidOperationException("Tour not found");

            // Check if this tour is already a featured tour
            var existing = await _featuredTourRepository.GetFeaturedTourByTourIdAsync(tourId);
            if (existing != null)
                throw new InvalidOperationException("This tour is already configured as a featured tour. Please use the edit function to update it.");

            // Check max limit (Featured Tours: max 2)
            var allFeaturedTours = await _featuredTourRepository.GetAllFeaturedToursAsync();
            if (allFeaturedTours.Count >= 2)
                throw new InvalidOperationException("Maximum 2 featured tours allowed. Please remove an existing featured tour first.");

            // Create new featured tour
            var maxOrder = await _featuredTourRepository.GetMaxDisplayOrderAsync();
            
            var newFeatured = new HomePageFeaturedTour
            {
                TourId = tourId,
                Category = dto.Badge,
                Title = dto.CategoryName,
                Description = dto.Description,
                ImageUrl = dto.ImageUrl,
                DisplayOrder = dto.DisplayOrder == 0 ? maxOrder + 1 : dto.DisplayOrder,
                CardClass = dto.CardClass,
                Active = dto.Active,
                UpdatedBy = userId,
                CreatedTime = DateTime.UtcNow
            };

            await _featuredTourRepository.CreateFeaturedTourAsync(newFeatured);
        }

        public async Task UpdateFeaturedTourAsync(int id, ConfigureFeaturedTourDto dto, string userId)
        {
            // Get the existing featured tour by ID
            var existing = await _featuredTourRepository.GetFeaturedTourByIdAsync(id);
            
            if (existing == null)
                throw new InvalidOperationException("Featured tour not found");

            // Update properties
            existing.Category = dto.Badge;
            existing.Title = dto.CategoryName;
            existing.Description = dto.Description;
            existing.ImageUrl = dto.ImageUrl;
            existing.DisplayOrder = dto.DisplayOrder;
            existing.CardClass = dto.CardClass;
            existing.Active = dto.Active;
            existing.UpdatedBy = userId;
            existing.UpdatedTime = DateTime.UtcNow;

            await _featuredTourRepository.UpdateFeaturedTourAsync(existing);
        }

        public async Task RemoveFeaturedTourAsync(int id, string userId)
        {
            var tour = await _featuredTourRepository.GetFeaturedTourByIdAsync(id);
            if (tour == null)
                throw new InvalidOperationException("Featured tour not found");

            tour.UpdatedBy = userId;
            await _featuredTourRepository.DeleteFeaturedTourAsync(id);
        }

        public async Task ReorderFeaturedToursAsync(List<ReorderTourDto> orders, string userId)
        {
            var tours = new List<HomePageFeaturedTour>();

            foreach (var order in orders)
            {
                var tour = await _featuredTourRepository.GetFeaturedTourByIdAsync(order.Id);
                if (tour != null)
                {
                    tour.DisplayOrder = order.DisplayOrder;
                    tour.UpdatedBy = userId;
                    tour.UpdatedTime = DateTime.UtcNow;
                    tours.Add(tour);
                }
            }

            await _featuredTourRepository.UpdateDisplayOrdersAsync(tours);
        }

        // Daily Tours
        public async Task<DailyToursConfigDto> GetDailyToursAsync()
        {
            var categories = await _categoryRepository.GetDailyTourCategoriesAsync();

            var toursDto = categories.Select(c => new DailyTourDto
            {
                Id = c.Id,
                CategoryId = c.Id,
                Badge = c.DailyTourBadge ?? string.Empty,
                CategoryName = c.CategoryName,
                Slug = c.Slug ?? "",
                Description = c.DailyTourDescription ?? string.Empty,
                ImageUrl = c.DailyTourImageUrl ?? string.Empty,
                DisplayOrder = c.DailyTourOrder ?? 0,
                CardClass = c.DailyTourCardClass
            }).ToList();

            return new DailyToursConfigDto
            {
                Tours = toursDto
            };
        }

        public async Task ConfigureDailyTourAsync(int categoryId, ConfigureDailyTourDto dto, string userId)
        {
            // Validate category exists
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new InvalidOperationException("Category not found");

            // Check if this category is already configured as a daily tour
            var wasAlreadyDailyTour = category.ShowInDailyTours == true;

            // Check max limit (Daily Tours: max 3) only if this is a new daily tour
            if (!wasAlreadyDailyTour)
            {
                var allDailyTours = await _categoryRepository.GetDailyTourCategoriesAsync();
                if (allDailyTours.Count >= 3)
                    throw new InvalidOperationException("Maximum 3 daily tours allowed. Please remove an existing daily tour first.");
            }

            // Update category with daily tour config
            category.DailyTourBadge = dto.Badge;
            category.DailyTourDescription = dto.Description;
            category.DailyTourImageUrl = dto.ImageUrl;
            category.DailyTourOrder = dto.DisplayOrder == 0 ? await _categoryRepository.GetMaxDailyTourOrderAsync() + 1 : dto.DisplayOrder;
            category.DailyTourCardClass = dto.CardClass;
            category.ShowInDailyTours = dto.Active;
            category.UpdatedBy = userId;
            category.UpdatedTime = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(category);
        }

        public async Task RemoveDailyTourAsync(int categoryId, string userId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new InvalidOperationException("Category not found");

            category.ShowInDailyTours = false;
            category.UpdatedBy = userId;
            category.UpdatedTime = DateTime.UtcNow;

            await _categoryRepository.UpdateAsync(category);
        }

        public async Task ReorderDailyToursAsync(List<ReorderTourDto> orders, string userId)
        {
            foreach (var order in orders)
            {
                var category = await _categoryRepository.GetByIdAsync(order.Id);
                if (category != null)
                {
                    category.DailyTourOrder = order.DisplayOrder;
                    category.UpdatedBy = userId;
                    category.UpdatedTime = DateTime.UtcNow;
                    await _categoryRepository.UpdateAsync(category);
                }
            }
        }
    }
}
