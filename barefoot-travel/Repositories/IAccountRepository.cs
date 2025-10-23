using barefoot_travel.DTOs;
using barefoot_travel.Models;

namespace barefoot_travel.Repositories
{
    public interface IAccountRepository
    {
        Task<Account?> GetUserByUsernameAsync(string username);
        Task<Account?> GetByIdAsync(int id);
        Task<List<Account>> GetAllAsync();
        Task<Account> CreateAsync(Account account);
        Task<Account> UpdateAsync(Account account);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        
        // Additional methods for UserService
        Task<Account?> GetByUsernameAsync(string username);
        Task<PagedResult<Account>> GetPagedAsync(int page, int pageSize, string? sortBy = null, string sortOrder = "asc", string? search = null, int? roleId = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    }
}
