using barefoot_travel.DTOs;
using barefoot_travel.DTOs.Category;

namespace barefoot_travel.Services
{
    public interface IHomePageService
    {
        Task<HomepageDataDto> GetHomepageSectionsAsync();
        Task ConfigureCategoryHomepageAsync(int categoryId, ConfigureHomepageDto dto, string userId);
        Task RemoveCategoryFromHomepageAsync(int categoryId, string userId);
        Task ReorderSectionsAsync(List<ReorderSectionDto> sections, string userId);
        Task<object> GetCategoryHomepageConfigAsync(int categoryId);
        Task<WaysToTravelConfigDto> GetWaysToTravelCategoriesAsync();
        Task ConfigureCategoryForWaysToTravelAsync(int categoryId, ConfigureWaysToTravelDto dto, string userId);
        Task RemoveCategoryFromWaysToTravelAsync(int categoryId, string userId);
        Task ReorderWaysToTravelCategoriesAsync(List<ReorderSectionDto> orders, string userId);
    }
}
