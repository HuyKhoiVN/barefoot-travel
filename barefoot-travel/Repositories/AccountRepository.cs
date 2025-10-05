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
    }
}
