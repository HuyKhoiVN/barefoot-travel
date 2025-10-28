using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public interface IHomePageSectionCategoryRepository
    {
        Task<List<HomePageSectionCategory>> GetBySectionIdAsync(int sectionId);
        Task<HomePageSectionCategory?> GetByIdAsync(int id);
        Task<bool> DeleteBySectionIdAsync(int sectionId);
        Task<bool> DeleteAsync(int id);
        Task<bool> CreateAsync(HomePageSectionCategory entity);
        Task<bool> CreateRangeAsync(List<HomePageSectionCategory> entities);
        Task<bool> UpdateAsync(HomePageSectionCategory entity);
        Task<bool> ExistsAsync(int sectionId, int categoryId);
    }
}

