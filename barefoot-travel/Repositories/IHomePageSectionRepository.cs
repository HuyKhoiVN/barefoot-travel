using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public interface IHomePageSectionRepository
    {
        Task<List<HomePageSection>> GetAllAsync();
        Task<HomePageSection?> GetByIdAsync(int id);
        Task<HomePageSection> CreateAsync(HomePageSection section);
        Task<HomePageSection> UpdateAsync(HomePageSection section);
        Task<bool> DeleteAsync(int id);
        Task<List<HomePageSection>> GetActiveSectionsOrderedAsync();
    }
}

