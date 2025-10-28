using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Section;
using barefoot_travel.Models;
using barefoot_travel.Repositories;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Services
{
    public class HomePageSectionService : IHomePageSectionService
    {
        private readonly SysDbContext _context;
        private readonly IHomePageSectionRepository _sectionRepository;
        private readonly IHomePageSectionCategoryRepository _sectionCategoryRepository;
        private readonly IHomePageSectionTourRepository _sectionTourRepository;
        private readonly ITourRepository _tourRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<HomePageSectionService> _logger;

        public HomePageSectionService(
            SysDbContext context,
            IHomePageSectionRepository sectionRepository,
            IHomePageSectionCategoryRepository sectionCategoryRepository,
            IHomePageSectionTourRepository sectionTourRepository,
            ITourRepository tourRepository,
            ICategoryRepository categoryRepository,
            ILogger<HomePageSectionService> logger)
        {
            _context = context;
            _sectionRepository = sectionRepository;
            _sectionCategoryRepository = sectionCategoryRepository;
            _sectionTourRepository = sectionTourRepository;
            _tourRepository = tourRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        #region Section CRUD Operations

        public async Task<List<HomePageSectionDto>> GetAllSectionsAsync()
        {
            try
            {
                var sections = await _sectionRepository.GetActiveSectionsOrderedAsync();
                var result = new List<HomePageSectionDto>();

                foreach (var section in sections)
                {
                    var dto = await MapToDto(section);
                    result.Add(dto);
                }

                _logger.LogInformation("Retrieved {Count} sections", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all sections");
                throw;
            }
        }

        public async Task<HomePageSectionDto?> GetSectionByIdAsync(int sectionId)
        {
            try
            {
                var section = await _sectionRepository.GetByIdAsync(sectionId);
                if (section == null)
                {
                    _logger.LogWarning("Section {SectionId} not found", sectionId);
                    return null;
                }

                return await MapToDto(section);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving section {SectionId}", sectionId);
                throw;
            }
        }

        public async Task<HomePageSectionDto> CreateSectionAsync(ConfigureHomePageSectionDto dto, string userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Validate input
                ValidateConfigureDto(dto);

                // Check for duplicate section name
                var existingSection = await (from s in _context.HomePageSections
                                            where s.SectionName == dto.SectionName && s.Active
                                            select s).FirstOrDefaultAsync();
                if (existingSection != null)
                {
                    throw new InvalidOperationException($"Section name '{dto.SectionName}' already exists");
                }

                // Create section entity
                var section = new HomePageSection
                {
                    SectionName = dto.SectionName,
                    HomepageTitle = dto.HomepageTitle,
                    LayoutStyle = dto.LayoutStyle,
                    MaxItems = dto.MaxItems,
                    DisplayOrder = dto.DisplayOrder,
                    IsActive = dto.IsActive,
                    BadgeText = dto.BadgeText,
                    CustomClass = dto.CustomClass,
                    SpotlightImageUrl = dto.SpotlightImageUrl,
                    SelectionMode = dto.SelectionMode,
                    PrimaryCategoryId = dto.PrimaryCategoryId,
                    CreatedTime = DateTime.UtcNow,
                    UpdatedBy = userId,
                    Active = true
                };

                // Enforce spotlight constraint
                if (section.LayoutStyle == "spotlight" && section.MaxItems > 3)
                {
                    section.MaxItems = 3;
                    _logger.LogWarning("Spotlight layout limited to 3 items for section {SectionName}", section.SectionName);
                }

                // Save section
                var createdSection = await _sectionRepository.CreateAsync(section);

                // Handle category relationships
                if (dto.SelectionMode == "auto" && dto.PrimaryCategoryId.HasValue)
                {
                    // Auto mode: Link primary category
                    await AddCategoryToSection(createdSection.Id, dto.PrimaryCategoryId.Value, 0);
                }
                else if (dto.SelectionMode == "manual" && dto.CategoryIds != null && dto.CategoryIds.Any())
                {
                    // Manual mode: Link multiple categories
                    var categoryEntities = dto.CategoryIds.Select((catId, index) => new HomePageSectionCategory
                    {
                        SectionId = createdSection.Id,
                        CategoryId = catId,
                        DisplayOrder = index,
                        Active = true
                    }).ToList();
                    await _sectionCategoryRepository.CreateRangeAsync(categoryEntities);
                }

                // Handle manually selected tours
                if (dto.SelectionMode == "manual" && dto.SelectedTourIds != null && dto.SelectedTourIds.Any())
                {
                    var tourEntities = dto.SelectedTourIds.Select((tourId, index) => new HomePageSectionTour
                    {
                        SectionId = createdSection.Id,
                        TourId = tourId,
                        DisplayOrder = index,
                        CreatedTime = DateTime.UtcNow,
                        UpdatedBy = userId,
                        Active = true
                    }).ToList();
                    await _sectionTourRepository.CreateRangeAsync(tourEntities);
                }

                await transaction.CommitAsync();
                
                _logger.LogInformation("Created section {SectionName} (ID: {SectionId}) by user {UserId}", 
                    section.SectionName, createdSection.Id, userId);

                // Reload with relationships
                var result = await _sectionRepository.GetByIdAsync(createdSection.Id);
                return await MapToDto(result!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating section {SectionName}", dto.SectionName);
                throw;
            }
        }

        public async Task<HomePageSectionDto> UpdateSectionAsync(int sectionId, ConfigureHomePageSectionDto dto, string userId)
        {
            var section = await _sectionRepository.GetByIdAsync(sectionId);
            if (section == null)
                throw new InvalidOperationException("Section not found");

            // Validate input
            ValidateConfigureDto(dto);

            // Update section properties
            section.SectionName = dto.SectionName;
            section.HomepageTitle = dto.HomepageTitle;
            section.LayoutStyle = dto.LayoutStyle;
            section.MaxItems = dto.MaxItems;
            section.DisplayOrder = dto.DisplayOrder;
            section.IsActive = dto.IsActive;
            section.BadgeText = dto.BadgeText;
            section.CustomClass = dto.CustomClass;
            section.SpotlightImageUrl = dto.SpotlightImageUrl;
            section.SelectionMode = dto.SelectionMode;
            section.PrimaryCategoryId = dto.PrimaryCategoryId;
            section.UpdatedBy = userId;

            // Enforce spotlight constraint
            if (section.LayoutStyle == "spotlight" && section.MaxItems > 3)
            {
                section.MaxItems = 3;
                _logger.LogWarning("Spotlight layout limited to 3 items for section {SectionId}", sectionId);
            }

            await _sectionRepository.UpdateAsync(section);

            // Update category relationships
            await _sectionCategoryRepository.DeleteBySectionIdAsync(sectionId);
            
            if (dto.SelectionMode == "auto" && dto.PrimaryCategoryId.HasValue)
            {
                await AddCategoryToSection(sectionId, dto.PrimaryCategoryId.Value, 0);
            }
            else if (dto.SelectionMode == "manual" && dto.CategoryIds != null && dto.CategoryIds.Any())
            {
                await AddCategoriesToSectionAsync(sectionId, dto.CategoryIds, userId);
            }

            // Update selected tours for manual mode
            if (dto.SelectionMode == "manual" && dto.SelectedTourIds != null)
            {
                await _sectionTourRepository.DeleteBySectionIdAsync(sectionId);
                if (dto.SelectedTourIds.Any())
                {
                    await AddToursToSectionAsync(sectionId, dto.SelectedTourIds, userId);
                }
            }

            // Reload with relationships
            var result = await _sectionRepository.GetByIdAsync(sectionId);
            return await MapToDto(result!);
        }

        public async Task<bool> DeleteSectionAsync(int sectionId, string userId)
        {
            var section = await _sectionRepository.GetByIdAsync(sectionId);
            if (section == null) return false;

            // Soft delete
            return await _sectionRepository.DeleteAsync(sectionId);
        }

        #endregion

        #region Section Ordering

        public async Task ReorderSectionsAsync(List<DTOs.Section.ReorderSectionDto> sections, string userId)
        {
            foreach (var item in sections)
            {
                var section = await _sectionRepository.GetByIdAsync(item.SectionId);
                if (section != null)
                {
                    section.DisplayOrder = item.DisplayOrder;
                    section.UpdatedBy = userId;
                    await _sectionRepository.UpdateAsync(section);
                }
            }
        }

        #endregion

        #region Section Tours

        public async Task<List<HomepageTourDto>> GetSectionToursAsync(int sectionId)
        {
            var section = await _sectionRepository.GetByIdAsync(sectionId);
            if (section == null)
                throw new InvalidOperationException("Section not found");

            return await GetToursForSection(section);
        }

        public async Task<SectionPreviewDto> GetSectionPreviewAsync(int sectionId)
        {
            var section = await _sectionRepository.GetByIdAsync(sectionId);
            if (section == null)
                throw new InvalidOperationException("Section not found");

            var tours = await GetToursForSection(section);

            return new SectionPreviewDto
            {
                SectionId = section.Id,
                HomepageTitle = section.HomepageTitle,
                LayoutStyle = section.LayoutStyle,
                SelectionMode = section.SelectionMode,
                Tours = tours
            };
        }

        #endregion

        #region Category Management

        public async Task AddCategoriesToSectionAsync(int sectionId, List<int> categoryIds, string userId)
        {
            var section = await _sectionRepository.GetByIdAsync(sectionId);
            if (section == null)
                throw new InvalidOperationException("Section not found");

            for (int i = 0; i < categoryIds.Count; i++)
            {
                await AddCategoryToSection(sectionId, categoryIds[i], i);
            }
        }

        public async Task RemoveCategoryFromSectionAsync(int sectionId, int categoryId)
        {
            var sectionCategories = await _sectionCategoryRepository.GetBySectionIdAsync(sectionId);
            var toRemove = sectionCategories.FirstOrDefault(sc => sc.CategoryId == categoryId);
            
            if (toRemove != null)
            {
                await _sectionCategoryRepository.DeleteAsync(toRemove.Id);
            }
        }

        #endregion

        #region Tour Management (Manual Mode)

        public async Task<List<HomepageTourDto>> GetAvailableToursForSectionAsync(int sectionId)
        {
            var section = await _sectionRepository.GetByIdAsync(sectionId);
            if (section == null)
                throw new InvalidOperationException("Section not found");

            // Get all categories linked to this section via repository
            var sectionCategories = await _sectionCategoryRepository.GetBySectionIdAsync(sectionId);
            if (!sectionCategories.Any())
                return new List<HomepageTourDto>();

            // Get tours from all linked categories
            var allTours = new List<HomepageTourDto>();
            foreach (var sc in sectionCategories)
            {
                var tours = await _tourRepository.GetToursByCategoryForHomepageAsync(sc.CategoryId, 100);
                allTours.AddRange(tours);
            }

            // Remove duplicates in memory (if a tour belongs to multiple categories)
            return allTours
                .GroupBy(t => t.Id)
                .Select(g => g.First())
                .ToList();
        }

        public async Task AddToursToSectionAsync(int sectionId, List<int> tourIds, string userId)
        {
            var section = await _sectionRepository.GetByIdAsync(sectionId);
            if (section == null)
                throw new InvalidOperationException("Section not found");

            // Delete existing tours
            await _sectionTourRepository.DeleteBySectionIdAsync(sectionId);

            // Add new tours
            var entities = tourIds.Select((tourId, index) => new HomePageSectionTour
            {
                SectionId = sectionId,
                TourId = tourId,
                DisplayOrder = index,
                CreatedTime = DateTime.UtcNow,
                UpdatedBy = userId,
                Active = true
            }).ToList();

            await _sectionTourRepository.CreateRangeAsync(entities);

            _logger.LogInformation("Added {Count} tours to section {SectionId}", tourIds.Count, sectionId);
        }

        public async Task RemoveTourFromSectionAsync(int sectionId, int tourId)
        {
            var sectionTours = await _sectionTourRepository.GetBySectionIdAsync(sectionId);
            var toRemove = sectionTours.FirstOrDefault(st => st.TourId == tourId);
            
            if (toRemove != null)
            {
                await _sectionTourRepository.DeleteAsync(toRemove.Id);
            }
        }

        public async Task ReorderSectionToursAsync(int sectionId, List<int> tourIds, string userId)
        {
            var sectionTours = await _sectionTourRepository.GetBySectionIdAsync(sectionId);

            for (int i = 0; i < tourIds.Count; i++)
            {
                var tourId = tourIds[i];
                var tour = sectionTours.FirstOrDefault(st => st.TourId == tourId);
                if (tour != null)
                {
                    tour.DisplayOrder = i;
                    tour.UpdatedBy = userId;
                    await _sectionTourRepository.UpdateAsync(tour);
                }
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task<HomePageSectionDto> MapToDto(HomePageSection section)
        {
            var dto = new HomePageSectionDto
            {
                Id = section.Id,
                SectionName = section.SectionName,
                HomepageTitle = section.HomepageTitle,
                LayoutStyle = section.LayoutStyle,
                MaxItems = section.MaxItems,
                DisplayOrder = section.DisplayOrder,
                IsActive = section.IsActive,
                BadgeText = section.BadgeText,
                CustomClass = section.CustomClass,
                SpotlightImageUrl = section.SpotlightImageUrl,
                SelectionMode = section.SelectionMode,
                PrimaryCategoryId = section.PrimaryCategoryId,
                CreatedTime = section.CreatedTime,
                UpdatedTime = section.UpdatedTime,
                UpdatedBy = section.UpdatedBy
            };

            // Get primary category name if exists
            if (section.PrimaryCategoryId.HasValue)
            {
                var primaryCategory = await _categoryRepository.GetByIdAsync(section.PrimaryCategoryId.Value);
                dto.PrimaryCategoryName = primaryCategory?.CategoryName;
            }

            // Get category information via repository
            var sectionCategories = await _sectionCategoryRepository.GetBySectionIdAsync(section.Id);
            dto.CategoryIds = sectionCategories.Select(sc => sc.CategoryId).ToList();

            // Get category names in correct order (avoid IndexOf in LINQ - SQL translation error)
            if (dto.CategoryIds.Any())
            {
                var categoryList = await (from c in _context.Categories
                                         where c.Active && dto.CategoryIds.Contains(c.Id)
                                         select new { c.Id, c.CategoryName })
                                         .ToListAsync();

                // Order by original CategoryIds order in memory (not in SQL)
                dto.CategoryNames = dto.CategoryIds
                    .Select(id => categoryList.FirstOrDefault(c => c.Id == id)?.CategoryName)
                    .Where(name => name != null)
                    .Cast<string>()
                    .ToList();
            }
            else
            {
                dto.CategoryNames = new List<string>();
            }

            // Get tours
            var tours = await GetToursForSection(section);
            dto.SelectedTours = tours;
            dto.TourCount = tours.Count;

            return dto;
        }

        private async Task<List<HomepageTourDto>> GetToursForSection(HomePageSection section)
        {
            if (section.SelectionMode == "manual")
            {
                // Manual mode: Use explicitly selected tours via repository
                var selectedTours = await _sectionTourRepository.GetBySectionIdAsync(section.Id);

                if (selectedTours.Any())
                {
                    var tourIds = selectedTours.Select(st => st.TourId).ToList();
                    
                    // Get tours by IDs and maintain the order
                    var tours = await _tourRepository.GetToursByIdsAsync(tourIds, section.MaxItems);
                    
                    // Reorder based on DisplayOrder (in memory to avoid SQL translation issues)
                    var orderedTours = tourIds
                        .Select(id => tours.FirstOrDefault(t => t.Id == id))
                        .Where(t => t != null)
                        .Cast<HomepageTourDto>()
                        .Take(section.MaxItems)
                        .ToList();
                    
                    return orderedTours;
                }

                return new List<HomepageTourDto>();
            }
            else
            {
                // Auto mode: Use category's tours
                if (section.PrimaryCategoryId.HasValue)
                {
                    return await _tourRepository.GetToursByCategoryForHomepageAsync(
                        section.PrimaryCategoryId.Value, 
                        section.MaxItems);
                }

                return new List<HomepageTourDto>();
            }
        }

        private async Task AddCategoryToSection(int sectionId, int categoryId, int displayOrder)
        {
            var entity = new HomePageSectionCategory
            {
                SectionId = sectionId,
                CategoryId = categoryId,
                DisplayOrder = displayOrder,
                Active = true
            };

            await _sectionCategoryRepository.CreateAsync(entity);
        }

        private void ValidateConfigureDto(ConfigureHomePageSectionDto dto)
        {
            // Validate layout style
            var validLayouts = new[] { "grid", "grid-2", "grid-3", "spotlight", "carousel" };
            if (!validLayouts.Contains(dto.LayoutStyle))
                throw new InvalidOperationException("Invalid layout style");

            // Validate selection mode
            if (dto.SelectionMode != "auto" && dto.SelectionMode != "manual")
                throw new InvalidOperationException("Selection mode must be 'auto' or 'manual'");

            // Validate auto mode requirements
            if (dto.SelectionMode == "auto" && !dto.PrimaryCategoryId.HasValue)
                throw new InvalidOperationException("Auto mode requires a primary category");

            // Validate manual mode requirements
            if (dto.SelectionMode == "manual" && (dto.CategoryIds == null || !dto.CategoryIds.Any()))
                throw new InvalidOperationException("Manual mode requires at least one category");
        }

        #endregion
    }
}

