using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly SysDbContext _context;

        public RoleRepository(SysDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByIdAsync(int id)
        {
            return await _context.Roles
                .Where(r => r.Id == id && r.Active)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Role>> GetAllAsync()
        {
            return await _context.Roles
                .Where(r => r.Active)
                .ToListAsync();
        }

        public async Task<Role> CreateAsync(Role role)
        {
            role.CreatedTime = DateTime.UtcNow;
            role.Active = true;
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<Role> UpdateAsync(Role role)
        {
            role.UpdatedTime = DateTime.UtcNow;
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
            return role;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role == null) return false;

            role.Active = false;
            role.UpdatedTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Roles
                .AnyAsync(r => r.Id == id && r.Active);
        }

        public async Task<Role?> GetByNameAsync(string roleName)
        {
            return await _context.Roles
                .Where(r => r.RoleName == roleName && r.Active)
                .FirstOrDefaultAsync();
        }
    }
}
