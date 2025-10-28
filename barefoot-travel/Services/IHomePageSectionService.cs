using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Section;

namespace barefoot_travel.Services
{
    public interface IHomePageSectionService
    {
        // Section CRUD operations
        Task<List<HomePageSectionDto>> GetAllSectionsAsync();
        Task<HomePageSectionDto?> GetSectionByIdAsync(int sectionId);
        Task<HomePageSectionDto> CreateSectionAsync(ConfigureHomePageSectionDto dto, string userId);
        Task<HomePageSectionDto> UpdateSectionAsync(int sectionId, ConfigureHomePageSectionDto dto, string userId);
        Task<bool> DeleteSectionAsync(int sectionId, string userId);
        
        // Section ordering
        Task ReorderSectionsAsync(List<DTOs.Section.ReorderSectionDto> sections, string userId);
        
        // Section tours
        Task<List<HomepageTourDto>> GetSectionToursAsync(int sectionId);
        Task<SectionPreviewDto> GetSectionPreviewAsync(int sectionId);
        
        // Category management for sections
        Task AddCategoriesToSectionAsync(int sectionId, List<int> categoryIds, string userId);
        Task RemoveCategoryFromSectionAsync(int sectionId, int categoryId);
        
        // Tour management for sections (manual mode)
        Task<List<HomepageTourDto>> GetAvailableToursForSectionAsync(int sectionId);
        Task AddToursToSectionAsync(int sectionId, List<int> tourIds, string userId);
        Task RemoveTourFromSectionAsync(int sectionId, int tourId);
        Task ReorderSectionToursAsync(int sectionId, List<int> tourIds, string userId);
    }
}

