using barefoot_travel.DTOs;
using barefoot_travel.Models;
using Microsoft.EntityFrameworkCore;

namespace barefoot_travel.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private readonly SysDbContext _context;

        public AccountRepository(SysDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetUserByUsernameAsync(string username)
        {
            return await _context.Accounts
                .Where(a => a.Username == username && a.Active)
                .FirstOrDefaultAsync();
        }

        public async Task<Account?> GetByIdAsync(int id)
        {
            return await _context.Accounts
                .Where(a => a.Id == id && a.Active)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Account>> GetAllAsync()
        {
            return await _context.Accounts
                .Where(a => a.Active)
                .ToListAsync();
        }

        public async Task<Account> CreateAsync(Account account)
        {
            account.CreatedTime = DateTime.UtcNow;
            account.Active = true;
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<Account> UpdateAsync(Account account)
        {
            account.UpdatedTime = DateTime.UtcNow;
            _context.Accounts.Update(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return false;

            account.Active = false;
            account.UpdatedTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Accounts
                .AnyAsync(a => a.Id == id && a.Active);
        }

        // Additional methods for UserService
        public async Task<Account?> GetByUsernameAsync(string username)
        {
            return await _context.Accounts
                .Where(a => a.Username == username)
                .FirstOrDefaultAsync();
        }

        public async Task<PagedResult<Account>> GetPagedAsync(int page, int pageSize, string? sortBy = null, string sortOrder = "asc", string? search = null, int? roleId = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            var query = _context.Accounts.Where(a => a.Active).AsQueryable();

            // Apply role filter
            if (roleId.HasValue)
            {
                query = query.Where(a => a.RoleId == roleId.Value);
            }

            // Apply date range filter
            if (dateFrom.HasValue)
            {
                query = query.Where(a => a.CreatedTime >= dateFrom.Value);
            }
            if (dateTo.HasValue)
            {
                query = query.Where(a => a.CreatedTime <= dateTo.Value);
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(a => 
                    a.Username.Contains(search) ||
                    a.FullName.Contains(search) ||
                    (a.Email != null && a.Email.Contains(search)) ||
                    (a.Phone != null && a.Phone.Contains(search)));
            }

            // Apply sorting
            query = sortBy?.ToLower() switch
            {
                "username" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(a => a.Username) : query.OrderBy(a => a.Username),
                "email" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(a => a.Email) : query.OrderBy(a => a.Email),
                "phone" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(a => a.Phone) : query.OrderBy(a => a.Phone),
                "createdtime" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(a => a.CreatedTime) : query.OrderBy(a => a.CreatedTime),
                "roleid" => sortOrder.ToLower() == "desc" ? query.OrderByDescending(a => a.RoleId) : query.OrderBy(a => a.RoleId),
                _ => sortOrder.ToLower() == "desc" ? query.OrderByDescending(a => a.CreatedTime) : query.OrderBy(a => a.CreatedTime)
            };

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Account>
            {
                Items = items,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }
    }
}
