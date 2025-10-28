using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Repositories
{
    public class HomePageSectionRepository : IHomePageSectionRepository
    {
        private readonly SysDbContext _context;

        public HomePageSectionRepository(SysDbContext context)
        {
            _context = context;
        }

        public async Task<List<HomePageSection>> GetAllAsync()
        {
            return await (from s in _context.HomePageSections
                         where s.Active
                         select s).ToListAsync();
        }

        public async Task<HomePageSection?> GetByIdAsync(int id)
        {
            return await (from s in _context.HomePageSections
                         where s.Id == id && s.Active
                         select s).FirstOrDefaultAsync();
        }

        public async Task<HomePageSection> CreateAsync(HomePageSection section)
        {
            section.CreatedTime = DateTime.UtcNow;
            section.Active = true;
            
            await _context.HomePageSections.AddAsync(section);
            await _context.SaveChangesAsync();
            
            return section;
        }

        public async Task<HomePageSection> UpdateAsync(HomePageSection section)
        {
            section.UpdatedTime = DateTime.UtcNow;
            
            _context.HomePageSections.Update(section);
            await _context.SaveChangesAsync();
            
            return section;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var section = await _context.HomePageSections.FindAsync(id);
                if (section == null) return false;

                // Soft delete
                section.Active = false;
                section.UpdatedTime = DateTime.UtcNow;
                
                _context.HomePageSections.Update(section);
                await _context.SaveChangesAsync();
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<HomePageSection>> GetActiveSectionsOrderedAsync()
        {
            return await (from s in _context.HomePageSections
                         where s.Active && s.IsActive
                         orderby s.DisplayOrder
                         select s).ToListAsync();
        }
    }
}

